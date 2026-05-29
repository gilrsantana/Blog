namespace Blog.Application.UseCases.Posts.Queries;

public record PostResponse(
    Guid Id,
    string Title,
    string Slug,
    string Summary,
    string Content,
    string Tags,
    string CoverImage,
    int ReadingTime,
    bool IsPublished,
    DateTime CreatedAt,
    Guid AuthorId);
