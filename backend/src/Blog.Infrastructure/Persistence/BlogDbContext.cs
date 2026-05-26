using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Blog.Domain.Entities;
using Blog.Infrastructure.Identity;

namespace Blog.Infrastructure.Persistence;

public class BlogDbContext : IdentityDbContext<
    Account, 
    Role, 
    Guid, 
    IdentityUserClaim<Guid>, 
    AccountRole, 
    IdentityUserLogin<Guid>, 
    IdentityRoleClaim<Guid>, 
    IdentityUserToken<Guid>>
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options)
        : base(options)
    {
    }

    public new DbSet<User> Users => Set<User>();
    public DbSet<Post> Posts => Set<Post>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Customize ASP.NET Identity Table Names
        builder.Entity<Account>(entity => entity.ToTable("Accounts"));
        builder.Entity<Role>(entity => entity.ToTable("Roles"));
        builder.Entity<AccountRole>(entity => entity.ToTable("AccountRoles"));
        builder.Entity<IdentityUserClaim<Guid>>(entity => entity.ToTable("AccountClaims"));
        builder.Entity<IdentityUserLogin<Guid>>(entity => entity.ToTable("AccountLogins"));
        builder.Entity<IdentityRoleClaim<Guid>>(entity => entity.ToTable("RoleClaims"));
        builder.Entity<IdentityUserToken<Guid>>(entity => entity.ToTable("AccountTokens"));

        // Domain User Configuration (Shared Primary Key 1:1 with Account)
        builder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            
            entity.Property(u => u.DisplayName).HasMaxLength(150).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.Property(u => u.Bio).HasMaxLength(1000);
            entity.Property(u => u.AvatarUrl).HasMaxLength(500);

            entity.HasOne<Account>()
                  .WithOne()
                  .HasForeignKey<User>(u => u.Id)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Domain Post Configuration
        builder.Entity<Post>(entity =>
        {
            entity.ToTable("Posts");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Title).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Slug).HasMaxLength(250).IsRequired();
            entity.Property(p => p.Summary).HasMaxLength(500).IsRequired();
            entity.Property(p => p.Content).HasColumnType("longtext").IsRequired();
            entity.Property(p => p.Tags).HasMaxLength(100);
            entity.Property(p => p.CoverImage).HasMaxLength(500);

            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(p => p.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(p => p.Slug).IsUnique();
        });
    }
}
