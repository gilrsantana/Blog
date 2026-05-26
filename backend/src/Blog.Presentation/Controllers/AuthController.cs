using Microsoft.AspNetCore.Mvc;
using Blog.Application.Common.CQRS;
using Blog.Application.Common.Interfaces;
using Blog.Application.UseCases.Accounts.Commands;

namespace Blog.Presentation.Controllers;

public class AuthController : ApiControllerBase
{
    private readonly ICommandHandler<LoginCommand, TokenResponse> _loginHandler;
    private readonly ICommandHandler<RefreshTokenCommand, TokenResponse> _refreshTokenHandler;
    private readonly ICommandHandler<UpdateProfileCommand> _updateProfileHandler;

    public AuthController(
        ICommandHandler<LoginCommand, TokenResponse> loginHandler,
        ICommandHandler<RefreshTokenCommand, TokenResponse> refreshTokenHandler,
        ICommandHandler<UpdateProfileCommand> updateProfileHandler)
    {
        _loginHandler = loginHandler;
        _refreshTokenHandler = refreshTokenHandler;
        _updateProfileHandler = updateProfileHandler;
    }

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
}
