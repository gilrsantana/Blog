using System;
using Blog.Application.Common.CQRS;

namespace Blog.Application.UseCases.Accounts.Commands;

public record UpdatePasswordCommand(Guid AccountId, string CurrentPassword, string NewPassword) : ICommand;
