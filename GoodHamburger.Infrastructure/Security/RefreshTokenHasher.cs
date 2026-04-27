using System.Security.Cryptography;
using System.Text;
using GoodHamburger.Application.Abstractions.Security;

namespace GoodHamburger.Infrastructure.Security;

internal sealed class RefreshTokenHasher : IRefreshTokenHasher
{
    public string Hash(string refreshToken)
    {
        var bytes = Encoding.UTF8.GetBytes(refreshToken);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public bool Verify(string refreshToken, string hash)
    {
        var computedHash = Hash(refreshToken);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(hash));
    }
}
