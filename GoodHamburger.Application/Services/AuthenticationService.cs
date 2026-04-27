using GoodHamburger.Application.Abstractions.Persistence;
using GoodHamburger.Application.Abstractions.Security;
using GoodHamburger.Application.Contracts;
using GoodHamburger.Application.Shared;
using GoodHamburger.Domain.Common;
using GoodHamburger.Domain.Users;

namespace GoodHamburger.Application.Services;

public interface IAuthenticationService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<Result<AuthResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);

    Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRefreshTokenHasher _refreshTokenHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly TimeProvider _timeProvider;

    public AuthenticationService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IRefreshTokenHasher refreshTokenHasher,
        IJwtTokenService jwtTokenService,
        TimeProvider timeProvider)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _refreshTokenHasher = refreshTokenHasher;
        _jwtTokenService = jwtTokenService;
        _timeProvider = timeProvider;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Result<AuthResponse>.Failure(
                new ApplicationError("invalid_email", "O email é obrigatório."));
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            return Result<AuthResponse>.Failure(
                new ApplicationError("invalid_password", "A senha deve ter pelo menos 6 caracteres."));
        }

        var existingUser = await _userRepository.GetByEmailAsync(request.Email.Trim(), cancellationToken);
        if (existingUser is not null)
        {
            return Result<AuthResponse>.Failure(
                new ApplicationError("email_already_exists", "Já existe um usuário com este email."));
        }

        var utcNow = _timeProvider.GetUtcNow();
        var passwordHash = _passwordHasher.Hash(request.Password);

        try
        {
            var user = User.Create(Guid.NewGuid(), request.Email.Trim(), passwordHash, utcNow);
            await _userRepository.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = await GenerateTokensAsync(user, utcNow, cancellationToken);
            return Result<AuthResponse>.Success(response);
        }
        catch (DomainException ex)
        {
            return Result<AuthResponse>.Failure(new ApplicationError(ex.Code, ex.Message));
        }
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Result<AuthResponse>.Failure(
                new ApplicationError("invalid_email", "O email é obrigatório."));
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<AuthResponse>.Failure(
                new ApplicationError("invalid_password", "A senha é obrigatória."));
        }

        var user = await _userRepository.GetByEmailAsync(request.Email.Trim(), cancellationToken);
        if (user is null)
        {
            return Result<AuthResponse>.Failure(
                new ApplicationError("invalid_credentials", "Credenciais inválidas."));
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result<AuthResponse>.Failure(
                new ApplicationError("invalid_credentials", "Credenciais inválidas."));
        }

        var utcNow = _timeProvider.GetUtcNow();
        var response = await GenerateTokensAsync(user, utcNow, cancellationToken);

        return Result<AuthResponse>.Success(response);
    }

    public async Task<Result<AuthResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.AccessToken))
        {
            return Result<AuthResponse>.Failure(
                new ApplicationError("invalid_access_token", "O access token é obrigatório."));
        }

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result<AuthResponse>.Failure(
                new ApplicationError("invalid_refresh_token", "O refresh token é obrigatório."));
        }

        // Extrai o userId do access token, mesmo que expirado
        var userId = _jwtTokenService.ExtractUserIdFromExpiredToken(request.AccessToken);
        if (!userId.HasValue)
        {
            return Result<AuthResponse>.Failure(
                new ApplicationError("invalid_access_token", "Access token inválido."));
        }

        // Busca o usuário pelo refresh token armazenado (comparação por hash)
        var refreshTokenHash = _refreshTokenHasher.Hash(request.RefreshToken);
        var user = await _userRepository.GetByRefreshTokenAsync(refreshTokenHash, cancellationToken);
        if (user is null || user.Id != userId.Value)
        {
            return Result<AuthResponse>.Failure(
                new ApplicationError("invalid_refresh_token", "Refresh token inválido."));
        }

        var storedToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshTokenHash);
        if (storedToken is null)
        {
            return Result<AuthResponse>.Failure(
                new ApplicationError("invalid_refresh_token", "Refresh token não encontrado."));
        }

        // DETECÇÃO DE REUSE: Se o token foi revogado OU substituído, indica possível roubo
        if (storedToken.IsRevoked || storedToken.IsReplaced)
        {
            // Segurança: revoga TODOS os tokens da mesma família (e todas as famílias do usuário)
            var utcNow = _timeProvider.GetUtcNow();
            user.RevokeAllRefreshTokens(utcNow);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<AuthResponse>.Failure(
                new ApplicationError("token_reuse_detected", "Possível roubo de token detectado. Faça login novamente."));
        }

        var now = _timeProvider.GetUtcNow();

        try
        {
            // Gera novos tokens
            var newAccessToken = _jwtTokenService.GenerateAccessToken(user, now, out var accessTokenExpiresAt);
            var newRefreshTokenValue = _jwtTokenService.GenerateRefreshToken();
            var newRefreshTokenHash = _refreshTokenHasher.Hash(newRefreshTokenValue);
            var refreshTokenExpiresAt = now.AddDays(7);

            // Adiciona o novo token mantendo a mesma família
            var newToken = user.AddRefreshToken(newRefreshTokenHash, storedToken.TokenFamily, refreshTokenExpiresAt, now);

            // Marca o token antigo como substituído e revogado
            storedToken.MarkAsReplaced(newToken.Id);
            storedToken.Revoke(now);

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<AuthResponse>.Success(new AuthResponse(newAccessToken, newRefreshTokenValue, accessTokenExpiresAt));
        }
        catch (DomainException ex)
        {
            return Result<AuthResponse>.Failure(new ApplicationError(ex.Code, ex.Message));
        }
    }

    public async Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result.Failure(
                new ApplicationError("invalid_refresh_token", "O refresh token é obrigatório."));
        }

        var refreshTokenHash = _refreshTokenHasher.Hash(refreshToken);
        var user = await _userRepository.GetByRefreshTokenAsync(refreshTokenHash, cancellationToken);
        if (user is null)
        {
            return Result.Success(); // Idempotente: se não encontrou, já está deslogado
        }

        var storedToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshTokenHash);
        if (storedToken is null || storedToken.IsRevoked)
        {
            return Result.Success();
        }

        var utcNow = _timeProvider.GetUtcNow();
        user.RevokeRefreshToken(storedToken.Id, utcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<AuthResponse> GenerateTokensAsync(User user, DateTimeOffset utcNow, CancellationToken cancellationToken)
    {
        var accessToken = _jwtTokenService.GenerateAccessToken(user, utcNow, out var accessTokenExpiresAt);
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();
        var refreshTokenHash = _refreshTokenHasher.Hash(refreshTokenValue);

        // Refresh token expira em 7 dias
        var refreshTokenExpiresAt = utcNow.AddDays(7);
        var tokenFamily = Guid.NewGuid();

        user.AddRefreshToken(refreshTokenHash, tokenFamily, refreshTokenExpiresAt, utcNow);
        
        // ✅ NÃO CHAME _userRepository.Update(user)!
        // Quando a entidade já está no tracking do EF (foi retornada pelo repositório)
        // Qualquer alteração é detectada AUTOMATICAMENTE.
        // Chamar .Update() aqui é o erro que causa DbUpdateConcurrencyException!
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(accessToken, refreshTokenValue, accessTokenExpiresAt);
    }
}
