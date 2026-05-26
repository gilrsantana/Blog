using Microsoft.AspNetCore.Identity;

namespace Blog.Infrastructure.Identity;

public class Role : IdentityRole<Guid>
{
    public string Description { get; private set; } = string.Empty;

    // Required for EF Core materialization
    private Role()
    {
    }

    private Role(string name, string description)
    {
        Id = Guid.NewGuid();
        Name = name;
        NormalizedName = name.ToUpperInvariant();
        Description = description;
    }

    public static Role Create(string name, string description)
    {
        return new Role(name, description);
    }
}
