using Microsoft.AspNetCore.Mvc;
using Blog.Application.Common.CQRS;
using Blog.Application.Common.Interfaces;
using Blog.Application.UseCases.Accounts.Commands;
using Microsoft.AspNetCore.Authorization;

namespace Blog.Presentation.Controllers;

public class AuthController : ApiControllerBase
{
    private readonly ICommandHandler<LoginCommand, TokenResponse> _loginHandler;
    private readonly ICommandHandler<RefreshTokenCommand, TokenResponse> _refreshTokenHandler;
    private readonly ICommandHandler<UpdateProfileCommand> _updateProfileHandler;
    private readonly ICommandHandler<RegisterCommand, Guid> _registerHandler;
    private readonly ICommandHandler<AssignRoleCommand> _assignRoleHandler;
    private readonly ICommandHandler<InactivateAccountCommand> _inactivateAccountHandler;
    private readonly ICommandHandler<DeleteAccountCommand> _deleteAccountHandler;
    private readonly ICommandHandler<UpdatePasswordCommand> _updatePasswordHandler;

    public AuthController(
        ICommandHandler<LoginCommand, TokenResponse> loginHandler,
        ICommandHandler<RefreshTokenCommand, TokenResponse> refreshTokenHandler,
        ICommandHandler<UpdateProfileCommand> updateProfileHandler,
        ICommandHandler<RegisterCommand, Guid> registerHandler,
        ICommandHandler<AssignRoleCommand> assignRoleHandler,
        ICommandHandler<InactivateAccountCommand> inactivateAccountHandler,
        ICommandHandler<DeleteAccountCommand> deleteAccountHandler,
        ICommandHandler<UpdatePasswordCommand> updatePasswordHandler)
    {
        _loginHandler = loginHandler;
        _refreshTokenHandler = refreshTokenHandler;
        _updateProfileHandler = updateProfileHandler;
        _registerHandler = registerHandler;
        _assignRoleHandler = assignRoleHandler;
        _inactivateAccountHandler = inactivateAccountHandler;
        _deleteAccountHandler = deleteAccountHandler;
        _updatePasswordHandler = updatePasswordHandler;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _loginHandler.HandleAsync(command);
        return HandleResult(result);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command)
    {
        var result = await _refreshTokenHandler.HandleAsync(command);
        return HandleResult(result);
    }

    [HttpPut("profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command)
    {
        var result = await _updateProfileHandler.HandleAsync(command);
        return HandleResult(result);
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await _registerHandler.HandleAsync(command);
        return HandleResult(result);
    }

    [HttpPost("roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleCommand command)
    {
        var result = await _assignRoleHandler.HandleAsync(command);
        return HandleResult(result);
    }


    [HttpPost("inactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inactivate([FromBody] InactivateAccountCommand command)
    {
        var result = await _inactivateAccountHandler.HandleAsync(command);
        return HandleResult(result);
    }

    [HttpDelete("account/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAccount(Guid id)
    {
        var command = new DeleteAccountCommand(id);
        var result = await _deleteAccountHandler.HandleAsync(command);
        return HandleResult(result);
    }

    [HttpPut("password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
    {
        var command = new UpdatePasswordCommand(request.AccountId, request.CurrentPassword, request.NewPassword);
        var result = await _updatePasswordHandler.HandleAsync(command);
        return HandleResult(result);
    }
}

public record UpdatePasswordRequest(Guid AccountId, string CurrentPassword, string NewPassword);
