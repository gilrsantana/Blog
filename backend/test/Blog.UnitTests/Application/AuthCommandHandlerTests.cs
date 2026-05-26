using Xunit;
using Blog.Application.Common.Interfaces;
using Blog.Application.UseCases.Accounts.Commands;
using Blog.Shared;

namespace Blog.UnitTests.Application;

public class AuthCommandHandlerTests
{
    private readonly FakeIdentityService _identityService = new();

    [Fact]
    public async Task Login_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        // Arrange
        var email = "admin@example.com";
        var password = "StrongPassword123!";
        await _identityService.RegisterAsync(Guid.NewGuid(), email, password);

        var command = new LoginCommand(email, password);
        var handler = new LoginCommandHandler(_identityService);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("access-token", result.Value.AccessToken);
        Assert.Equal("refresh-token", result.Value.RefreshToken);
    }

    [Fact]
    public async Task Login_ShouldReturnFailure_WhenCredentialsAreInvalid()
    {
        // Arrange
        var command = new LoginCommand("wrong@example.com", "wrongpassword");
        var handler = new LoginCommandHandler(_identityService);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidCredentials", result.Error.Code);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnNewTokens_WhenTokensAreValid()
    {
        // Arrange
        var command = new RefreshTokenCommand("expired-access", "valid-refresh");
        var handler = new RefreshTokenCommandHandler(_identityService);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("new-access-token", result.Value.AccessToken);
        Assert.Equal("new-refresh-token", result.Value.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnFailure_WhenTokensAreInvalid()
    {
        // Arrange
        _identityService.RefreshShouldFail = true;
        var command = new RefreshTokenCommand("expired-access", "invalid-refresh");
        var handler = new RefreshTokenCommandHandler(_identityService);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidToken", result.Error.Code);
    }
}
