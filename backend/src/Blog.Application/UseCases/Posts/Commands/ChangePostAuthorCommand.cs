using Blog.Application.Common.CQRS;

namespace Blog.Application.UseCases.Posts.Commands;

public record ChangePostAuthorCommand(Guid PostId, Guid NewAuthorId) : ICommand;
