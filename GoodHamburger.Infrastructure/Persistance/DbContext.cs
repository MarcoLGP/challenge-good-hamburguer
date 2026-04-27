using GoodHamburger.Application.Abstractions.Persistence;
using GoodHamburger.Domain.Menu;
using GoodHamburger.Domain.Orders;
using GoodHamburger.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GoodHamburger.Infrastructure.Persistence;

public sealed class GoodHamburgerDbContext(DbContextOptions<GoodHamburgerDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var nullableMenuItemConverter = new ValueConverter<MenuItemCode?, string?>(
            v => v.HasValue ? v.Value.ToString() : null,
            v => string.IsNullOrWhiteSpace(v) ? null : Enum.Parse<MenuItemCode>(v));

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();

            entity.Property(x => x.SandwichCode)
                .HasConversion(nullableMenuItemConverter)
                .HasMaxLength(32);

            entity.Property(x => x.SideCode)
                .HasConversion(nullableMenuItemConverter)
                .HasMaxLength(32);

            entity.Property(x => x.DrinkCode)
                .HasConversion(nullableMenuItemConverter)
                .HasMaxLength(32);

            entity.Property(x => x.Subtotal).HasPrecision(18, 2);
            entity.Property(x => x.DiscountRate).HasPrecision(5, 4);
            entity.Property(x => x.Discount).HasPrecision(18, 2);
            entity.Property(x => x.Total).HasPrecision(18, 2);

            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();

            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Email).IsRequired().HasMaxLength(256);
            entity.Property(x => x.PasswordHash).IsRequired().HasMaxLength(256);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();

            entity.HasMany(x => x.RefreshTokens)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();

            entity.Property(x => x.Token).IsRequired().HasMaxLength(64); // SHA256 hex = 64 chars
            entity.HasIndex(x => x.Token);
            entity.Property(x => x.TokenFamily).IsRequired();
            entity.HasIndex(x => x.TokenFamily);
            entity.Property(x => x.ExpiresAt).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
        });
    }
}
