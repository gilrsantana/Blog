using Microsoft.AspNetCore.Mvc;
using Blog.Shared;

namespace Blog.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok();
        }

        return MapFailureResult(result);
    }

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return MapFailureResult(result);
    }

    private IActionResult MapFailureResult(Result result)
    {
        return result.Error.Code switch
        {
            "Auth.InvalidCredentials" => Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = result.Error.Message
            }),
            "Auth.InvalidToken" or "Token.Expired" => Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid Token",
                Detail = result.Error.Message
            }),
            _ when result.Error.Code.Contains("NotFound") => NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = result.Error.Message
            }),
            _ when result.Error.Code.Contains("Required") || result.Error.Code.Contains("Invalid") => BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = result.Error.Message
            }),
            _ => BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Error",
                Detail = result.Error.Message
            })
        };
    }
}
