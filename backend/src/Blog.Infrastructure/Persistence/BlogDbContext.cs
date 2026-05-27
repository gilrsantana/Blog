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
        builder.ApplyConfigurationsFromAssembly(typeof(BlogDbContext).Assembly);
    }
}
