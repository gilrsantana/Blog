using Blog.Application.UseCases.Posts.Commands;
using Blog.Domain.Entities;
using Blog.Application.UseCases.Posts.Handlers;

namespace Blog.UnitTests.Application;

public class CreatePostCommandHandlerTests
{
    private readonly FakePostRepository _postRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly CreatePostCommandHandler _handler;

    public CreatePostCommandHandlerTests()
    {
        _handler = new CreatePostCommandHandler(_postRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreatePostCommand(
            "New Post Title",
            "new-post-title",
            "Post summary",
            "This is post content",
            "dotnet",
            "image.png",
            Guid.NewGuid()
        );

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        Assert.Single(_postRepository.Posts);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSlugIsNotUnique()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var existingPost = Post.Create("Title", "duplicate-slug", "Summary", "Content", "tags", "image", authorId).Value;
        await _postRepository.AddAsync(existingPost);

        var command = new CreatePostCommand(
            "New Title",
            "duplicate-slug",
            "Summary",
            "Content",
            "tags",
            "image",
            authorId
        );

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Post.SlugNotUnique", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }
}
