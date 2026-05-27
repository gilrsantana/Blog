using System;
using Blog.Application.Common.CQRS;

namespace Blog.Application.UseCases.Accounts.Commands;

public record InactivateAccountCommand(Guid AccountId) : ICommand;
