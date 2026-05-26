using Blog.Shared;

namespace Blog.Domain.Entities;

public class Post
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string Slug { get; private set; }
    public string Summary { get; private set; }
    public string Content { get; private set; }
    public string Tags { get; private set; }
    public string CoverImage { get; private set; }
    public int ReadingTime { get; private set; }
    public bool IsPublished { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid AuthorId { get; private set; }

    // Required for EF Core migrations/materialization
    private Post()
    {
        Title = string.Empty;
        Slug = string.Empty;
        Summary = string.Empty;
        Content = string.Empty;
        Tags = string.Empty;
        CoverImage = string.Empty;
    }

    private Post(Guid id, string title, string slug, string summary, string content, string tags, string coverImage, Guid authorId)
    {
        Id = id;
        Title = title;
        Slug = slug;
        Summary = summary;
        Content = content;
        Tags = tags;
        CoverImage = coverImage;
        AuthorId = authorId;
        IsPublished = false;
        CreatedAt = DateTime.UtcNow;
        ReadingTime = CalculateReadingTime(content);
    }

    public static Result<Post> Create(string title, string slug, string summary, string content, string tags, string coverImage, Guid authorId)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure<Post>(new Error("Post.TitleRequired", "Title is required."));
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            return Result.Failure<Post>(new Error("Post.SlugRequired", "Slug is required."));
        }

        if (string.IsNullOrWhiteSpace(summary))
        {
            return Result.Failure<Post>(new Error("Post.SummaryRequired", "Summary is required."));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return Result.Failure<Post>(new Error("Post.ContentRequired", "Content is required."));
        }

        if (authorId == Guid.Empty)
        {
            return Result.Failure<Post>(new Error("Post.InvalidAuthorId", "Author ID is required."));
        }

        return new Post(Guid.NewGuid(), title, slug, summary, content, tags ?? string.Empty, coverImage ?? string.Empty, authorId);
    }

    public Result Update(string title, string slug, string summary, string content, string tags, string coverImage)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure(new Error("Post.TitleRequired", "Title is required."));
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            return Result.Failure(new Error("Post.SlugRequired", "Slug is required."));
        }

        if (string.IsNullOrWhiteSpace(summary))
        {
            return Result.Failure(new Error("Post.SummaryRequired", "Summary is required."));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return Result.Failure(new Error("Post.ContentRequired", "Content is required."));
        }

        Title = title;
        Slug = slug;
        Summary = summary;
        Content = content;
        Tags = tags ?? string.Empty;
        CoverImage = coverImage ?? string.Empty;
        ReadingTime = CalculateReadingTime(content);

        return Result.Success();
    }

    public Result ChangeAuthor(Guid newAuthorId)
    {
        if (newAuthorId == Guid.Empty)
        {
            return Result.Failure(new Error("Post.InvalidAuthorId", "New Author ID is invalid."));
        }

        AuthorId = newAuthorId;
        return Result.Success();
    }

    public void Inactivate()
    {
        IsPublished = false;
    }

    public void Publish()
    {
        IsPublished = true;
    }

    private static int CalculateReadingTime(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return 0;

        const int WordsPerMinute = 200;
        int wordCount = content.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
        
        int minutes = (int)Math.Ceiling(wordCount / (double)WordsPerMinute);
        return Math.Max(1, minutes);
    }
}
