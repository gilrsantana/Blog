using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using Blog.Shared;

namespace Blog.UnitTests;

public class FakePostRepository : IPostRepository
{
    public List<Post> Posts { get; } = new();
    public bool HasAssociatedPostsResult { get; set; } = false;

    public Task<Post?> GetByIdAsync(Guid id, CancellationToken ct = default) => 
        Task.FromResult(Posts.FirstOrDefault(p => p.Id == id));

    public Task<Post?> GetBySlugAsync(string slug, CancellationToken ct = default) => 
        Task.FromResult(Posts.FirstOrDefault(p => p.Slug == slug));

    public Task<List<Post>> GetAllAsync(CancellationToken ct = default) => 
        Task.FromResult(Posts.ToList());

    public Task AddAsync(Post post, CancellationToken ct = default)
    {
        Posts.Add(post);
        return Task.CompletedTask;
    }

    public void Update(Post post) {}
    public void Delete(Post post) { Posts.Remove(post); }

    public Task<bool> HasAssociatedPostsAsync(Guid authorId, CancellationToken ct = default) =>
        Task.FromResult(HasAssociatedPostsResult);
}

public class FakeUserRepository : IUserRepository
{
    public List<User> Users { get; } = new();

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) => 
        Task.FromResult(Users.FirstOrDefault(u => u.Id == id));

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) => 
        Task.FromResult(Users.FirstOrDefault(u => u.Email == email));

    public Task AddAsync(User user, CancellationToken ct = default)
    {
        Users.Add(user);
        return Task.CompletedTask;
    }

    public void Update(User user) {}
    public void Delete(User user) { Users.Remove(user); }
}

public class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        SaveCount++;
        return Task.FromResult(1);
    }
}

public class FakeIdentityService : IIdentityService
{
    public Dictionary<string, string> Users { get; } = new(); // email -> password
    public List<Guid> RegisteredIds { get; } = new();
    public bool LoginShouldFail { get; set; }
    public bool RefreshShouldFail { get; set; }

    public Task<Result<TokenResponse>> LoginAsync(string email, string password)
    {
        if (LoginShouldFail || !Users.TryGetValue(email, out var pw) || pw != password)
        {
            return Task.FromResult(Result.Failure<TokenResponse>(new Error("Auth.InvalidCredentials", "Invalid credentials.")));
        }
        return Task.FromResult(Result.Success(new TokenResponse("access-token", "refresh-token")));
    }

    public Task<Result<TokenResponse>> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        if (RefreshShouldFail)
        {
            return Task.FromResult(Result.Failure<TokenResponse>(new Error("Auth.InvalidToken", "Invalid refresh token.")));
        }
        return Task.FromResult(Result.Success(new TokenResponse("new-access-token", "new-refresh-token")));
    }

    public async Task<Result> RegisterAsync(Guid id, string email, string password)
    {
        Users[email] = password;
        RegisteredIds.Add(id);
        await AssignRoleAsync(id, "Reader");
        return Result.Success();
    }

    public Task<Result> UpdatePasswordAsync(Guid accountId, string currentPassword, string newPassword) =>
        Task.FromResult(Result.Success());

    public Task<Result> DeleteAccountAsync(Guid accountId) =>
        Task.FromResult(Result.Success());

    public Task<Result> InactivateAccountAsync(Guid accountId) =>
        Task.FromResult(Result.Success());

    public Dictionary<Guid, List<string>> UserRoles { get; } = new();

    public Task<Result> AssignRoleAsync(Guid accountId, string roleName)
    {
        if (!UserRoles.ContainsKey(accountId))
        {
            UserRoles[accountId] = new List<string>();
        }
        UserRoles[accountId].Add(roleName);
        return Task.FromResult(Result.Success());
    }
}
