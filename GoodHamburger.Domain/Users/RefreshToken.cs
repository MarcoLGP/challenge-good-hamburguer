namespace GoodHamburger.Domain.Users;

/// <summary>
/// Representa um token de refresh armazenado no banco de dados.
/// Permite renovar o access token sem exigir credenciais novamente.
/// </summary>
public sealed class RefreshToken
{
    /// <summary>
    /// Identificador único do token de refresh.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// ID do usuário proprietário do token.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// O valor do token (normalmente um hash ou UUID).
    /// </summary>
    public string Token { get; private set; } = string.Empty;

    /// <summary>
    /// Data/hora de expiração do token em UTC.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; private set; }

    /// <summary>
    /// Identificador da família de tokens para detectar reuse.
    /// </summary>
    public Guid TokenFamily { get; private set; }

    /// <summary>
    /// ID do token que substituiu este (para rastreamento de rotação).
    /// </summary>
    public Guid? ReplacedByTokenId { get; private set; }

    /// <summary>
    /// Data/hora em que o token foi revogado (null se ativo).
    /// </summary>
    public DateTimeOffset? RevokedAt { get; private set; }

    /// <summary>
    /// Indica se o token foi revogado.
    /// </summary>
    public bool IsRevoked => RevokedAt.HasValue;

    /// <summary>
    /// Indica se o token expirou.
    /// </summary>
    public bool IsExpired(DateTimeOffset utcNow) => utcNow >= ExpiresAt;

    /// <summary>
    /// Indica se o token é válido (ativo e não expirado).
    /// </summary>
    public bool IsValid(DateTimeOffset utcNow) => !IsRevoked && !IsExpired(utcNow);

    /// <summary>
    /// Indica se este token foi substituído por outro.
    /// </summary>
    public bool IsReplaced => ReplacedByTokenId.HasValue;

    /// <summary>
    /// Data/hora de criação do token em UTC.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Usuário proprietário do token (navegação EF Core).
    /// </summary>
    public User User { get; private set; } = null!;

    // Construtor privado para EF Core
    private RefreshToken()
    {
    }

    /// <summary>
    /// Factory method para criar um novo refresh token.
    /// </summary>
    /// <param name="id">Identificador único do token.</param>
    /// <param name="userId">ID do usuário proprietário.</param>
    /// <param name="token">Valor do token (hash).</param>
    /// <param name="tokenFamily">Família de tokens para rastreamento.</param>
    /// <param name="expiresAt">Data/hora de expiração.</param>
    /// <param name="utcNow">Data/hora atual em UTC.</param>
    /// <returns>Novo RefreshToken criado.</returns>
    /// <exception cref="InvalidUserException">Se parâmetros são inválidos.</exception>
    public static RefreshToken Create(
        Guid id,
        Guid userId,
        string token,
        Guid tokenFamily,
        DateTimeOffset expiresAt,
        DateTimeOffset utcNow)
    {
        if (id == Guid.Empty)
            throw new InvalidUserException("O identificador do token é inválido.");

        if (userId == Guid.Empty)
            throw new InvalidUserException("O identificador do usuário é inválido.");

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidUserException("O token não pode estar vazio.");

        if (tokenFamily == Guid.Empty)
            throw new InvalidUserException("A família de tokens é inválida.");

        if (expiresAt <= utcNow)
            throw new InvalidUserException("O token não pode expirar no passado.");

        return new RefreshToken
        {
            Id = id,
            UserId = userId,
            Token = token,
            TokenFamily = tokenFamily,
            ExpiresAt = expiresAt,
            CreatedAt = utcNow,
            RevokedAt = null
        };
    }

    /// <summary>
    /// Marca este token como substituído por um novo token.
    /// </summary>
    /// <param name="replacementTokenId">ID do token que substituiu este.</param>
    public void MarkAsReplaced(Guid replacementTokenId)
    {
        if (replacementTokenId == Guid.Empty)
            throw new InvalidUserException("O identificador do token substituto é inválido.");

        ReplacedByTokenId = replacementTokenId;
    }

    /// <summary>
    /// Revoga o token, marcando-o como inválido.
    /// </summary>
    /// <param name="utcNow">Data/hora atual em UTC.</param>
    public void Revoke(DateTimeOffset utcNow)
    {
        if (IsRevoked)
            return;

        RevokedAt = utcNow;
    }
}
