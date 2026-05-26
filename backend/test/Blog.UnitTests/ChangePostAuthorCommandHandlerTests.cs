using Xunit;
using Blog.Application.Common.Interfaces;
using Blog.Application.UseCases.Posts.Commands;
using Blog.Domain.Entities;
using Blog.Shared;

namespace Blog.UnitTests;

public class ChangePostAuthorCommandHandlerTests
{
    private readonly FakePostRepository _postRepository = new();
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly ChangePostAuthorCommandHandler _handler;

    public ChangePostAuthorCommandHandlerTests()
    {
        _handler = new ChangePostAuthorCommandHandler(_postRepository, _userRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenParametersAreValid()
    {
        // Arrange
        var oldAuthorId = Guid.NewGuid();
        var newAuthorId = Guid.NewGuid();
        
        var post = Post.Create("Title", "slug", "Summary", "Content", "tags", "image", oldAuthorId).Value;
        await _postRepository.AddAsync(post);

        var newUser = User.Create(newAuthorId, "newauthor@example.com", "New Author", "Bio", "Avatar").Value;
        await _userRepository.AddAsync(newUser);

        var command = new ChangePostAuthorCommand(post.Id, newAuthorId);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newAuthorId, post.AuthorId);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPostDoesNotExist()
    {
        // Arrange
        var command = new ChangePostAuthorCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Post.NotFound", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNewAuthorDoesNotExist()
    {
        // Arrange
        var oldAuthorId = Guid.NewGuid();
        var post = Post.Create("Title", "slug", "Summary", "Content", "tags", "image", oldAuthorId).Value;
        await _postRepository.AddAsync(post);

        var command = new ChangePostAuthorCommand(post.Id, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.NotFound", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNewAuthorIsInactive()
    {
        // Arrange
        var oldAuthorId = Guid.NewGuid();
        var newAuthorId = Guid.NewGuid();
        
        var post = Post.Create("Title", "slug", "Summary", "Content", "tags", "image", oldAuthorId).Value;
        await _postRepository.AddAsync(post);

        var newUser = User.Create(newAuthorId, "newauthor@example.com", "New Author", "Bio", "Avatar").Value;
        newUser.Inactivate();
        await _userRepository.AddAsync(newUser);

        var command = new ChangePostAuthorCommand(post.Id, newAuthorId);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.Inactive", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
