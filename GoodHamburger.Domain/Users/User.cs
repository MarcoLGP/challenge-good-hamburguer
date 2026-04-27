namespace GoodHamburger.Domain.Users;

/// <summary>
/// Representa um usuário do sistema com credenciais de autenticação.
/// </summary>
public sealed class User
{
    /// <summary>
    /// Identificador único do usuário.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Email único do usuário.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Hash da senha do usuário (nunca armazenar senha em plano texto).
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// Data/hora de criação do usuário em UTC.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Data/hora da última atualização em UTC.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Tokens de refresh associados a este usuário.
    /// </summary>
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    // Construtor privado para EF Core
    private User()
    {
    }

    /// <summary>
    /// Factory method para criar um novo usuário.
    /// </summary>
    /// <param name="id">Identificador único do usuário.</param>
    /// <param name="email">Email do usuário.</param>
    /// <param name="passwordHash">Hash da senha gerado via bcrypt ou similar.</param>
    /// <param name="utcNow">Data/hora atual em UTC.</param>
    /// <returns>Novo usuário criado.</returns>
    /// <exception cref="InvalidUserException">Se email é inválido ou vazio.</exception>
    public static User Create(Guid id, string email, string passwordHash, DateTimeOffset utcNow)
    {
        if (id == Guid.Empty)
            throw new InvalidUserException("O identificador do usuário é inválido.");

        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidUserException("O email do usuário é obrigatório.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new InvalidUserException("O hash da senha é obrigatório.");

        return new User
        {
            Id = id,
            Email = email.Trim(),
            PasswordHash = passwordHash,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }

    /// <summary>
    /// Adiciona um novo refresh token ao usuário.
    /// </summary>
    /// <param name="token">Token (hash) para armazenar.</param>
    /// <param name="tokenFamily">Família de tokens para rastreamento.</param>
    /// <param name="expiresAt">Data/hora de expiração do token.</param>
    /// <param name="utcNow">Data/hora atual em UTC.</param>
    /// <returns>O RefreshToken criado.</returns>
    public RefreshToken AddRefreshToken(string token, Guid tokenFamily, DateTimeOffset expiresAt, DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidUserException("O token de refresh não pode estar vazio.");

        if (tokenFamily == Guid.Empty)
            throw new InvalidUserException("A família de tokens é inválida.");

        if (expiresAt <= utcNow)
            throw new InvalidUserException("O token de refresh não pode expirar no passado.");

        var refreshToken = RefreshToken.Create(Guid.NewGuid(), this.Id, token, tokenFamily, expiresAt, utcNow);
        RefreshTokens.Add(refreshToken);
        UpdatedAt = utcNow;

        return refreshToken;
    }

    /// <summary>
    /// Revoga um refresh token específico.
    /// </summary>
    /// <param name="tokenId">ID do token a revogar.</param>
    /// <param name="utcNow">Data/hora atual em UTC.</param>
    public void RevokeRefreshToken(Guid tokenId, DateTimeOffset utcNow)
    {
        var token = RefreshTokens.FirstOrDefault(rt => rt.Id == tokenId);
        if (token != null)
        {
            token.Revoke(utcNow);
            UpdatedAt = utcNow;
        }
    }

    /// <summary>
    /// Revoga todos os refresh tokens do usuário.
    /// </summary>
    /// <param name="utcNow">Data/hora atual em UTC.</param>
    public void RevokeAllRefreshTokens(DateTimeOffset utcNow)
    {
        foreach (var token in RefreshTokens.Where(rt => !rt.IsRevoked))
        {
            token.Revoke(utcNow);
        }
        UpdatedAt = utcNow;
    }
}
