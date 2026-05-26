using Microsoft.AspNetCore.Mvc;
using Blog.Application.Common.CQRS;
using Blog.Application.UseCases.Posts.Commands;

namespace Blog.Presentation.Controllers;

public class PostsController : ApiControllerBase
{
    private readonly ICommandHandler<CreatePostCommand, Guid> _createPostHandler;
    private readonly ICommandHandler<ChangePostAuthorCommand> _changeAuthorHandler;

    public PostsController(
        ICommandHandler<CreatePostCommand, Guid> createPostHandler,
        ICommandHandler<ChangePostAuthorCommand> changeAuthorHandler)
    {
        _createPostHandler = createPostHandler;
        _changeAuthorHandler = changeAuthorHandler;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePostCommand command)
    {
        var result = await _createPostHandler.HandleAsync(command);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(Create), new { id = result.Value }, result.Value);
        }

        return HandleResult(result);
    }

    [HttpPut("{id}/author")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeAuthor(Guid id, [FromBody] ChangePostAuthorRequest request)
    {
        var command = new ChangePostAuthorCommand(id, request.NewAuthorId);
        var result = await _changeAuthorHandler.HandleAsync(command);
        return HandleResult(result);
    }
}

public record ChangePostAuthorRequest(Guid NewAuthorId);
