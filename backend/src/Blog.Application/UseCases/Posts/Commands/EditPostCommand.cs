using System;
using Blog.Application.Common.CQRS;

namespace Blog.Application.UseCases.Posts.Commands;

public record EditPostCommand(
    Guid Id,
    string Title,
    string Slug,
    string Summary,
    string Content,
    string Tags,
    string CoverImage) : ICommand;
