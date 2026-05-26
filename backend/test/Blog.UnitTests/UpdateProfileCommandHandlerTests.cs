using Xunit;
using Blog.Application.Common.Interfaces;
using Blog.Application.UseCases.Accounts.Commands;
using Blog.Domain.Entities;
using Blog.Shared;

namespace Blog.UnitTests;

public class UpdateProfileCommandHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly UpdateProfileCommandHandler _handler;

    public UpdateProfileCommandHandlerTests()
    {
        _handler = new UpdateProfileCommandHandler(_userRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenParametersAreValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create(userId, "old@example.com", "Old Name", "Old Bio", "Old Avatar").Value;
        await _userRepository.AddAsync(user);

        var command = new UpdateProfileCommand(userId, "New Name", "New Bio", "New Avatar");

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("New Name", user.DisplayName);
        Assert.Equal("New Bio", user.Bio);
        Assert.Equal("New Avatar", user.AvatarUrl);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new UpdateProfileCommand(Guid.NewGuid(), "New Name", "New Bio", "New Avatar");

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.NotFound", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
