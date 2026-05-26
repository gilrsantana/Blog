using Blog.Application.Common.CQRS;

namespace Blog.Application.UseCases.Posts.Commands;

public record CreatePostCommand(
    string Title,
    string Slug,
    string Summary,
    string Content,
    string Tags,
    string CoverImage,
    Guid AuthorId) : ICommand<Guid>;
