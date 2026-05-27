using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Blog.Domain.Entities;
using Blog.Infrastructure.Identity;

namespace Blog.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.DisplayName).HasMaxLength(150).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.Bio).HasMaxLength(1000);
        builder.Property(u => u.AvatarUrl).HasMaxLength(500);

        builder.HasOne<Account>()
              .WithOne()
              .HasForeignKey<User>(u => u.Id)
              .OnDelete(DeleteBehavior.Cascade);
    }
}
