using Microsoft.AspNetCore.Mvc;
using Blog.Application.Common.CQRS;
using Blog.Application.UseCases.Posts.Commands;
using Blog.Application.UseCases.Posts.Queries;

namespace Blog.Presentation.Controllers;

public class PostsController : ApiControllerBase
{
    private readonly ICommandHandler<CreatePostCommand, Guid> _createPostHandler;
    private readonly ICommandHandler<ChangePostAuthorCommand> _changeAuthorHandler;
    private readonly ICommandHandler<EditPostCommand> _editPostHandler;
    private readonly ICommandHandler<DeletePostCommand> _deletePostHandler;
    private readonly ICommandHandler<InactivatePostCommand> _inactivatePostHandler;
    private readonly IQueryHandler<GetPostBySlugQuery, PostResponse> _getPostBySlugHandler;
    private readonly IQueryHandler<GetPostsQuery, PagedResponse<PostResponse>> _getPostsHandler;

    public PostsController(
        ICommandHandler<CreatePostCommand, Guid> createPostHandler,
        ICommandHandler<ChangePostAuthorCommand> changeAuthorHandler,
        ICommandHandler<EditPostCommand> editPostHandler,
        ICommandHandler<DeletePostCommand> deletePostHandler,
        ICommandHandler<InactivatePostCommand> inactivatePostHandler,
        IQueryHandler<GetPostBySlugQuery, PostResponse> getPostBySlugHandler,
        IQueryHandler<GetPostsQuery, PagedResponse<PostResponse>> getPostsHandler)
    {
        _createPostHandler = createPostHandler;
        _changeAuthorHandler = changeAuthorHandler;
        _editPostHandler = editPostHandler;
        _deletePostHandler = deletePostHandler;
        _inactivatePostHandler = inactivatePostHandler;
        _getPostBySlugHandler = getPostBySlugHandler;
        _getPostsHandler = getPostsHandler;
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

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Edit(Guid id, [FromBody] EditPostRequest request)
    {
        var command = new EditPostCommand(id, request.Title, request.Slug, request.Summary, request.Content, request.Tags, request.CoverImage);
        var result = await _editPostHandler.HandleAsync(command);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeletePostCommand(id);
        var result = await _deletePostHandler.HandleAsync(command);
        return HandleResult(result);
    }

    [HttpPut("{id}/inactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inactivate(Guid id)
    {
        var command = new InactivatePostCommand(id);
        var result = await _inactivatePostHandler.HandleAsync(command);
        return HandleResult(result);
    }

    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(PostResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, [FromQuery] bool isAdmin = false)
    {
        var query = new GetPostBySlugQuery(slug, isAdmin);
        var result = await _getPostBySlugHandler.HandleAsync(query);
        return HandleResult(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<PostResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] string? tag, [FromQuery] string? searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool isAdmin = false)
    {
        var query = new GetPostsQuery(tag, searchTerm, page, pageSize, isAdmin);
        var result = await _getPostsHandler.HandleAsync(query);
        return HandleResult(result);
    }
}

public record ChangePostAuthorRequest(Guid NewAuthorId);
public record EditPostRequest(string Title, string Slug, string Summary, string Content, string Tags, string CoverImage);
