using System;
using Blog.Application.Common.CQRS;

namespace Blog.Application.UseCases.Posts.Commands;

public record DeletePostCommand(Guid Id) : ICommand;
