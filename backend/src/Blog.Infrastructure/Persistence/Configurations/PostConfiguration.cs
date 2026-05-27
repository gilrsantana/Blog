using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Blog.Domain.Entities;

namespace Blog.Infrastructure.Persistence.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("Posts");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Slug).HasMaxLength(250).IsRequired();
        builder.Property(p => p.Summary).HasMaxLength(500).IsRequired();
        builder.Property(p => p.Content).HasColumnType("longtext").IsRequired();
        builder.Property(p => p.Tags).HasMaxLength(100);
        builder.Property(p => p.CoverImage).HasMaxLength(500);

        builder.HasOne<User>()
              .WithMany()
              .HasForeignKey(p => p.AuthorId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.Slug).IsUnique();
    }
}
