using Blog.Domain.Entities;

namespace Blog.UnitTests.Domain;

public class UserTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenParametersAreValid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var email = "test@example.com";
        var displayName = "Test User";
        var bio = "A developer bio";
        var avatarUrl = "https://example.com/avatar.png";

        // Act
        var result = User.Create(id, email, displayName, bio, avatarUrl);

        // Assert
        Assert.True(result.IsSuccess);
        var user = result.Value;
        Assert.Equal(id, user.Id);
        Assert.Equal(email, user.Email);
        Assert.Equal(displayName, user.DisplayName);
        Assert.Equal(bio, user.Bio);
        Assert.Equal(avatarUrl, user.AvatarUrl);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenEmailIsInvalid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var email = "invalid-email";
        var displayName = "Test User";

        // Act
        var result = User.Create(id, email, displayName, string.Empty, string.Empty);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.InvalidEmail", result.Error.Code);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenDisplayNameIsEmpty()
    {
        // Arrange
        var id = Guid.NewGuid();
        var email = "test@example.com";
        var displayName = "";

        // Act
        var result = User.Create(id, email, displayName, string.Empty, string.Empty);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.DisplayNameRequired", result.Error.Code);
    }

    [Fact]
    public void UpdateProfile_ShouldModifyProfileDetails()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = User.Create(id, "test@example.com", "Test User", "Old Bio", "Old Avatar").Value;

        // Act
        var result = user.UpdateProfile("New User Name", "New Bio", "New Avatar");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("New User Name", user.DisplayName);
        Assert.Equal("New Bio", user.Bio);
        Assert.Equal("New Avatar", user.AvatarUrl);
    }

    [Fact]
    public void Inactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = User.Create(id, "test@example.com", "Test User", "Bio", "Avatar").Value;

        // Act
        user.Inactivate();

        // Assert
        Assert.False(user.IsActive);
    }
}
