using Blog.Application.Common.CQRS;

namespace Blog.Application.UseCases.Posts.Commands;

public record InactivatePostCommand(Guid Id) : ICommand;
