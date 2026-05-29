using Blog.Application.Common.CQRS;

namespace Blog.Application.UseCases.Accounts.Commands;

public record DeleteAccountCommand(Guid AccountId) : ICommand;
