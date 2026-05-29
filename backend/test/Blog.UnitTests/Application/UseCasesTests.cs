using Blog.Application.UseCases.Accounts.Commands;
using Blog.Application.UseCases.Accounts.Handlers;
using Blog.Application.UseCases.Posts.Commands;
using Blog.Application.UseCases.Posts.Handlers;
using Blog.Application.UseCases.Posts.Queries;
using Blog.Domain.Entities;

namespace Blog.UnitTests.Application;

public class UseCasesTests
{
    private readonly FakePostRepository _postRepository = new();
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeIdentityService _identityService = new();

    [Fact]
    public async Task Register_ShouldCreateUserAndIdentity_WhenCommandIsValid()
    {
        var handler = new RegisterCommandHandler(_identityService, _userRepository, _unitOfWork);
        var command = new RegisterCommand("new@user.com", "Password123!", "New User", "My bio", "avatar.jpg");

        var result = await handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Single(_userRepository.Users);
        Assert.Equal("new@user.com", _userRepository.Users.First().Email);
        Assert.Contains("Reader", _identityService.UserRoles.Values.First());
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Register_ShouldFail_WhenEmailIsAlreadyRegistered()
    {
        var handler = new RegisterCommandHandler(_identityService, _userRepository, _unitOfWork);
        var existingUser = User.Create(Guid.NewGuid(), "existing@user.com", "Existing", "bio", "avatar").Value;
        await _userRepository.AddAsync(existingUser);

        var command = new RegisterCommand("existing@user.com", "Password123!", "New User", "bio", "avatar");

        var result = await handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal("User.EmailNotUnique", result.Error.Code);
    }

    [Fact]
    public async Task AssignRole_ShouldCallIdentityService_WhenValid()
    {
        var handler = new AssignRoleCommandHandler(_identityService);
        var accountId = Guid.NewGuid();
        var command = new AssignRoleCommand(accountId, "Administrator");

        var result = await handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Contains("Administrator", _identityService.UserRoles[accountId]);
    }

    [Fact]
    public async Task InactivateAccount_ShouldInactivateUserAndIdentity_WhenValid()
    {
        var handler = new InactivateAccountCommandHandler(_identityService, _userRepository, _unitOfWork);
        var accountId = Guid.NewGuid();
        var user = User.Create(accountId, "user@test.com", "User", "bio", "avatar").Value;
        await _userRepository.AddAsync(user);

        var command = new InactivateAccountCommand(accountId);

        var result = await handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.False(user.IsActive);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task DeleteAccount_ShouldDeleteUserAndCredentials_WhenUserHasNoPosts()
    {
        var handler = new DeleteAccountCommandHandler(_identityService, _userRepository, _postRepository, _unitOfWork);
        var accountId = Guid.NewGuid();
        var user = User.Create(accountId, "user@test.com", "User", "bio", "avatar").Value;
        await _userRepository.AddAsync(user);

        _postRepository.HasAssociatedPostsResult = false;
        var command = new DeleteAccountCommand(accountId);

        var result = await handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Empty(_userRepository.Users);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task DeleteAccount_ShouldFail_WhenUserHasAssociatedPosts()
    {
        var handler = new DeleteAccountCommandHandler(_identityService, _userRepository, _postRepository, _unitOfWork);
        var accountId = Guid.NewGuid();
        var user = User.Create(accountId, "user@test.com", "User", "bio", "avatar").Value;
        await _userRepository.AddAsync(user);

        _postRepository.HasAssociatedPostsResult = true;
        var command = new DeleteAccountCommand(accountId);

        var result = await handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal("Auth.CannotDeleteAccountWithPosts", result.Error.Code);
        Assert.Single(_userRepository.Users);
    }

    [Fact]
    public async Task UpdatePassword_ShouldSucceed_WhenValid()
    {
        var handler = new UpdatePasswordCommandHandler(_identityService);
        var command = new UpdatePasswordCommand(Guid.NewGuid(), "OldPassword1!", "NewPassword1!");

        var result = await handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task EditPost_ShouldModifyFields_WhenValid()
    {
        var handler = new EditPostCommandHandler(_postRepository, _unitOfWork);
        var authorId = Guid.NewGuid();
        var post = Post.Create("Old Title", "old-slug", "Old Summary", "Old Content", "tags", "image.png", authorId).Value;
        await _postRepository.AddAsync(post);

        var command = new EditPostCommand(post.Id, "New Title", "new-slug", "New Summary", "New Content", "new-tags", "new-image.png");

        var result = await handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Equal("New Title", post.Title);
        Assert.Equal("new-slug", post.Slug);
        Assert.Equal("New Summary", post.Summary);
        Assert.Equal("New Content", post.Content);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task DeletePost_ShouldRemovePost_WhenValid()
    {
        var handler = new DeletePostCommandHandler(_postRepository, _unitOfWork);
        var authorId = Guid.NewGuid();
        var post = Post.Create("Title", "slug", "Summary", "Content", "tags", "image.png", authorId).Value;
        await _postRepository.AddAsync(post);

        var command = new DeletePostCommand(post.Id);

        var result = await handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Empty(_postRepository.Posts);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task InactivatePost_ShouldSetIsPublishedToFalse_WhenValid()
    {
        var handler = new InactivatePostCommandHandler(_postRepository, _unitOfWork);
        var authorId = Guid.NewGuid();
        var post = Post.Create("Title", "slug", "Summary", "Content", "tags", "image.png", authorId).Value;
        post.Publish();
        await _postRepository.AddAsync(post);

        var command = new InactivatePostCommand(post.Id);

        var result = await handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.False(post.IsPublished);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task GetPostBySlug_ShouldReturnPost_WhenPublishedAndGuest()
    {
        var handler = new GetPostBySlugQueryHandler(_postRepository);
        var authorId = Guid.NewGuid();
        var post = Post.Create("Title", "my-slug", "Summary", "Content", "tags", "image.png", authorId).Value;
        post.Publish();
        await _postRepository.AddAsync(post);

        var query = new GetPostBySlugQuery("my-slug", IsAdmin: false);

        var result = await handler.HandleAsync(query);

        Assert.True(result.IsSuccess);
        Assert.Equal("Title", result.Value.Title);
    }

    [Fact]
    public async Task GetPostBySlug_ShouldFail_WhenUnpublishedAndGuest()
    {
        var handler = new GetPostBySlugQueryHandler(_postRepository);
        var authorId = Guid.NewGuid();
        var post = Post.Create("Title", "my-slug", "Summary", "Content", "tags", "image.png", authorId).Value;
        await _postRepository.AddAsync(post);

        var query = new GetPostBySlugQuery("my-slug", IsAdmin: false);

        var result = await handler.HandleAsync(query);

        Assert.True(result.IsFailure);
        Assert.Equal("Post.Unpublished", result.Error.Code);
    }

    [Fact]
    public async Task GetPostBySlug_ShouldSucceed_WhenUnpublishedAndAdmin()
    {
        var handler = new GetPostBySlugQueryHandler(_postRepository);
        var authorId = Guid.NewGuid();
        var post = Post.Create("Title", "my-slug", "Summary", "Content", "tags", "image.png", authorId).Value;
        await _postRepository.AddAsync(post);

        var query = new GetPostBySlugQuery("my-slug", IsAdmin: true);

        var result = await handler.HandleAsync(query);

        Assert.True(result.IsSuccess);
        Assert.Equal("Title", result.Value.Title);
    }

    [Fact]
    public async Task GetPosts_ShouldReturnPublishedPostsOnly_WhenGuest()
    {
        var handler = new GetPostsQueryHandler(_postRepository);
        var authorId = Guid.NewGuid();
        var post1 = Post.Create("Title 1", "slug-1", "Summary 1", "Content 1", "dotnet", "image.png", authorId).Value;
        post1.Publish();
        var post2 = Post.Create("Title 2", "slug-2", "Summary 2", "Content 2", "java", "image.png", authorId).Value;
        await _postRepository.AddAsync(post1);
        await _postRepository.AddAsync(post2);

        var query = new GetPostsQuery(Page: 1, PageSize: 10, IsAdmin: false);

        var result = await handler.HandleAsync(query);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.Equal("Title 1", result.Value.Items.First().Title);
    }

    [Fact]
    public async Task GetPosts_ShouldReturnAllPosts_WhenAdmin()
    {
        var handler = new GetPostsQueryHandler(_postRepository);
        var authorId = Guid.NewGuid();
        var post1 = Post.Create("Title 1", "slug-1", "Summary 1", "Content 1", "dotnet", "image.png", authorId).Value;
        post1.Publish();
        var post2 = Post.Create("Title 2", "slug-2", "Summary 2", "Content 2", "java", "image.png", authorId).Value;
        await _postRepository.AddAsync(post1);
        await _postRepository.AddAsync(post2);

        var query = new GetPostsQuery(Page: 1, PageSize: 10, IsAdmin: true);

        var result = await handler.HandleAsync(query);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCount);
    }
}
