using GoodHamburger.Application.Abstractions.Persistence;
using GoodHamburger.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace GoodHamburger.Infrastructure.Persistence;

internal sealed class UserRepository(GoodHamburgerDbContext dbContext) : IUserRepository
{
    private readonly GoodHamburgerDbContext _dbContext = dbContext;

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        => await _dbContext.Users
            .Include(x => x.RefreshTokens)
            .FirstOrDefaultAsync(x => x.RefreshTokens.Any(rt => rt.Token == refreshToken), cancellationToken);

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
        => _dbContext.Users.AddAsync(user, cancellationToken).AsTask();

    public void Update(User user)
        => _dbContext.Users.Update(user);
}
