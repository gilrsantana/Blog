using System.Net;
using System.Net.Http.Json;
using Blog.Application.Common.Interfaces;
using Blog.Application.UseCases.Posts.Queries;

namespace Blog.IntegrationTests;

public class AuthAndPostIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthAndPostIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RunFullUseCasesIntegrationFlow_ShouldSucceed()
    {
        // 1. Register Account (UC1)
        var uniqueEmail = $"integration_test_{DateTime.UtcNow.Ticks}@blog.com";
        var registerPayload = new
        {
            email = uniqueEmail,
            password = "SecurePassword123!",
            displayName = "Integration Tester",
            bio = "Created during integration testing",
            avatarUrl = "http://example.com/avatar.png"
        };

        var registerRes = await _client.PostAsJsonAsync("/api/Auth/register", registerPayload);
        Assert.Equal(HttpStatusCode.OK, registerRes.StatusCode);
        var accountId = await registerRes.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, accountId);

        // 2. Login Account (UC2)
        var loginPayload = new
        {
            email = uniqueEmail,
            password = "SecurePassword123!"
        };
        var loginRes = await _client.PostAsJsonAsync("/api/Auth/login", loginPayload);
        Assert.Equal(HttpStatusCode.OK, loginRes.StatusCode);
        var tokens = await loginRes.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(tokens);
        Assert.NotEmpty(tokens.AccessToken);

        // 3. Update User Profile (UC9)
        var profilePayload = new
        {
            userId = accountId,
            displayName = "Integration Tester Updated",
            bio = "Updated during integration testing",
            avatarUrl = "http://example.com/new-avatar.png"
        };
        var profileRes = await _client.PutAsJsonAsync("/api/Auth/profile", profilePayload);
        Assert.Equal(HttpStatusCode.OK, profileRes.StatusCode);

        // 4. Assign Administrator Role (UC3)
        var rolePayload = new
        {
            accountId = accountId,
            roleName = "Administrator"
        };
        var roleRes = await _client.PostAsJsonAsync("/api/Auth/roles", rolePayload);
        Assert.Equal(HttpStatusCode.OK, roleRes.StatusCode);

        // 5. Create Post (UC4)
        var slug = $"integration-test-post-{DateTime.UtcNow.Ticks}";
        var postPayload = new
        {
            title = "Integration Test Post",
            slug = slug,
            summary = "Summary of post",
            content = "This is the content of the post",
            tags = "test, dotnet",
            coverImage = "http://example.com/cover.png",
            authorId = accountId
        };
        var postRes = await _client.PostAsJsonAsync("/api/Posts", postPayload);
        Assert.Equal(HttpStatusCode.Created, postRes.StatusCode);
        var postId = await postRes.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, postId);

        // 6. Read Post by Slug (UC7)
        var readRes = await _client.GetAsync($"/api/Posts/{slug}?isAdmin=true");
        Assert.Equal(HttpStatusCode.OK, readRes.StatusCode);
        var postDetails = await readRes.Content.ReadFromJsonAsync<PostResponse>();
        Assert.NotNull(postDetails);
        Assert.Equal("Integration Test Post", postDetails.Title);

        // 7. Edit Post (UC5)
        var editPayload = new
        {
            title = "Integration Test Post Updated",
            slug = slug,
            summary = "Updated summary",
            content = "Updated content",
            tags = "test, dotnet, updated",
            coverImage = "http://example.com/new-cover.png"
        };
        var editRes = await _client.PutAsJsonAsync($"/api/Posts/{postId}", editPayload);
        Assert.Equal(HttpStatusCode.OK, editRes.StatusCode);

        // 8. Inactivate Post (UC12)
        var inactivatePostRes = await _client.PutAsync($"/api/Posts/{postId}/inactivate", null);
        Assert.Equal(HttpStatusCode.OK, inactivatePostRes.StatusCode);

        // 9. List Posts (UC8)
        var listRes = await _client.GetAsync($"/api/Posts?page=1&pageSize=10&isAdmin=true");
        Assert.Equal(HttpStatusCode.OK, listRes.StatusCode);
        var pagedPosts = await listRes.Content.ReadFromJsonAsync<PagedResponse<PostResponse>>();
        Assert.NotNull(pagedPosts);
        Assert.True(pagedPosts.TotalCount >= 1);

        // 10. Delete Post (UC6)
        var deletePostRes = await _client.DeleteAsync($"/api/Posts/{postId}");
        Assert.Equal(HttpStatusCode.OK, deletePostRes.StatusCode);

        // 11. Inactivate Account (UC10)
        var inactivatePayload = new
        {
            accountId = accountId
        };
        var inactivateRes = await _client.PostAsJsonAsync("/api/Auth/inactivate", inactivatePayload);
        Assert.Equal(HttpStatusCode.OK, inactivateRes.StatusCode);

        // 12. Delete Account Completely (UC11)
        var deleteAccountRes = await _client.DeleteAsync($"/api/Auth/account/{accountId}");
        Assert.Equal(HttpStatusCode.OK, deleteAccountRes.StatusCode);
    }

    [Fact]
    public async Task LoginAndRefreshTokenFlow_ShouldSucceedAndRotateTokens()
    {
        // 1. Register Account
        var uniqueEmail = $"refresh_test_{DateTime.UtcNow.Ticks}@blog.com";
        var registerPayload = new
        {
            email = uniqueEmail,
            password = "SecurePassword123!",
            displayName = "Refresh Tester",
            bio = "Created during refresh token testing",
            avatarUrl = "http://example.com/avatar.png"
        };

        var registerRes = await _client.PostAsJsonAsync("/api/Auth/register", registerPayload);
        Assert.Equal(HttpStatusCode.OK, registerRes.StatusCode);
        var accountId = await registerRes.Content.ReadFromJsonAsync<Guid>();
        Assert.NotEqual(Guid.Empty, accountId);

        // 2. Login Account
        var loginPayload = new
        {
            email = uniqueEmail,
            password = "SecurePassword123!"
        };
        var loginRes = await _client.PostAsJsonAsync("/api/Auth/login", loginPayload);
        Assert.Equal(HttpStatusCode.OK, loginRes.StatusCode);
        var tokens1 = await loginRes.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(tokens1);
        Assert.NotEmpty(tokens1.AccessToken);
        Assert.NotEmpty(tokens1.RefreshToken);

        // 3. Refresh Token
        var refreshPayload = new
        {
            accessToken = tokens1.AccessToken,
            refreshToken = tokens1.RefreshToken
        };
        var refreshRes = await _client.PostAsJsonAsync("/api/Auth/refresh", refreshPayload);
        Assert.Equal(HttpStatusCode.OK, refreshRes.StatusCode);
        var tokens2 = await refreshRes.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(tokens2);
        Assert.NotEmpty(tokens2.AccessToken);
        Assert.NotEmpty(tokens2.RefreshToken);
        Assert.NotEqual(tokens1.AccessToken, tokens2.AccessToken);
        Assert.NotEqual(tokens1.RefreshToken, tokens2.RefreshToken);

        // 4. Try refreshing again with the old refresh token - should fail (rotation/revocation)
        var replayRes = await _client.PostAsJsonAsync("/api/Auth/refresh", refreshPayload);
        Assert.Equal(HttpStatusCode.Unauthorized, replayRes.StatusCode);

        // 5. Clean up Account
        var deleteAccountRes = await _client.DeleteAsync($"/api/Auth/account/{accountId}");
        Assert.Equal(HttpStatusCode.OK, deleteAccountRes.StatusCode);
    }
}
