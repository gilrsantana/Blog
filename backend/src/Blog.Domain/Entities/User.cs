using Blog.Shared;

namespace Blog.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string DisplayName { get; private set; }
    public string Bio { get; private set; }
    public string AvatarUrl { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Required for EF Core migrations/materialization
    private User()
    {
        Email = string.Empty;
        DisplayName = string.Empty;
        Bio = string.Empty;
        AvatarUrl = string.Empty;
    }

    private User(Guid id, string email, string displayName, string bio, string avatarUrl)
    {
        Id = id;
        Email = email;
        DisplayName = displayName;
        Bio = bio;
        AvatarUrl = avatarUrl;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<User> Create(Guid id, string email, string displayName, string bio, string avatarUrl)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure<User>(new Error("User.IdRequired", "User ID is required."));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            return Result.Failure<User>(new Error("User.DisplayNameRequired", "Display name is required."));
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return Result.Failure<User>(new Error("User.InvalidEmail", "A valid email is required."));
        }

        return new User(id, email, displayName, bio ?? string.Empty, avatarUrl ?? string.Empty);
    }

    public Result UpdateProfile(string displayName, string bio, string avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return Result.Failure(new Error("User.DisplayNameRequired", "Display name cannot be empty."));
        }

        DisplayName = displayName;
        Bio = bio ?? string.Empty;
        AvatarUrl = avatarUrl ?? string.Empty;

        return Result.Success();
    }

    public void Inactivate()
    {
        IsActive = false;
    }
}
