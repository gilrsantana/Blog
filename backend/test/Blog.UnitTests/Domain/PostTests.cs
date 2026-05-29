using Blog.Domain.Entities;

namespace Blog.UnitTests.Domain;

public class PostTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenParametersAreValid()
    {
        // Arrange
        var title = "Construindo APIs robustas com .NET 10";
        var slug = "construindo-apis-robustas-com-net-10";
        var summary = "Neste artigo aprenderemos como estruturar APIs robustas e elegantes...";
        var content = "O .NET 10 traz uma gama de novidades para desenvolvimento web..."; // 10 words
        var tags = "dotnet,webapi";
        var coverImage = "https://example.com/cover.png";
        var authorId = Guid.NewGuid();

        // Act
        var result = Post.Create(title, slug, summary, content, tags, coverImage, authorId);

        // Assert
        Assert.True(result.IsSuccess);
        var post = result.Value;
        Assert.Equal(title, post.Title);
        Assert.Equal(slug, post.Slug);
        Assert.Equal(summary, post.Summary);
        Assert.Equal(content, post.Content);
        Assert.Equal(tags, post.Tags);
        Assert.Equal(coverImage, post.CoverImage);
        Assert.Equal(authorId, post.AuthorId);
        Assert.False(post.IsPublished);
        Assert.Equal(1, post.ReadingTime); // 10 words / 200 WPM = 0.05 -> ceiling = 1 minute
    }

    [Fact]
    public void Create_ShouldCalculateReadingTimeCorrectly_ForLongContent()
    {
        // Arrange
        var content = string.Join(" ", Enumerable.Repeat("word", 450)); // 450 words
        var authorId = Guid.NewGuid();

        // Act
        var result = Post.Create("Title", "title", "Summary", content, "tag", "image", authorId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.ReadingTime); // 450 / 200 = 2.25 -> ceiling = 3 minutes
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenTitleIsEmpty()
    {
        // Act
        var result = Post.Create("", "slug", "Summary", "Content", "tags", "image", Guid.NewGuid());

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Post.TitleRequired", result.Error.Code);
    }

    [Fact]
    public void Update_ShouldModifyFieldsAndRecalculateReadingTime()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var post = Post.Create("Title", "slug", "Summary", "Short content", "tags", "image", authorId).Value;
        var longContent = string.Join(" ", Enumerable.Repeat("word", 250)); // 250 words -> 2 mins

        // Act
        var result = post.Update("New Title", "new-slug", "New Summary", longContent, "new-tags", "new-image");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("New Title", post.Title);
        Assert.Equal(longContent, post.Content);
        Assert.Equal(2, post.ReadingTime);
    }

    [Fact]
    public void ChangeAuthor_ShouldUpdateAuthorId()
    {
        // Arrange
        var oldAuthorId = Guid.NewGuid();
        var newAuthorId = Guid.NewGuid();
        var post = Post.Create("Title", "slug", "Summary", "Content", "tags", "image", oldAuthorId).Value;

        // Act
        var result = post.ChangeAuthor(newAuthorId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newAuthorId, post.AuthorId);
    }

    [Fact]
    public void ChangeAuthor_ShouldReturnFailure_WhenAuthorIdIsEmpty()
    {
        // Arrange
        var post = Post.Create("Title", "slug", "Summary", "Content", "tags", "image", Guid.NewGuid()).Value;

        // Act
        var result = post.ChangeAuthor(Guid.Empty);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Post.InvalidAuthorId", result.Error.Code);
    }
}
