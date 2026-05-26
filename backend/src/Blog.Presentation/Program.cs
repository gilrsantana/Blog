using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Blog.Application.Common.CQRS;
using Blog.Application.Common.Interfaces;
using Blog.Application.UseCases.Accounts.Commands;
using Blog.Application.UseCases.Posts.Commands;
using Blog.Domain.Entities;
using Blog.Infrastructure.Identity;
using Blog.Infrastructure.Persistence;
using Blog.Infrastructure.Persistence.Repositories;
using Blog.Presentation.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// DbContext & MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=localhost;Database=blog_db;User=root;Password=;";
builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ASP.NET Identity Core Configuration
builder.Services.AddIdentityCore<Account>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
})
.AddRoles<Role>()
.AddEntityFrameworkStores<BlogDbContext>();

// Repositories & Shared Interfaces
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IIdentityService, IdentityService>();

// CQRS Commands Handlers DI registration
builder.Services.AddScoped<ICommandHandler<CreatePostCommand, Guid>, CreatePostCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ChangePostAuthorCommand>, ChangePostAuthorCommandHandler>();
builder.Services.AddScoped<ICommandHandler<LoginCommand, TokenResponse>, LoginCommandHandler>();
builder.Services.AddScoped<ICommandHandler<RefreshTokenCommand, TokenResponse>, RefreshTokenCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateProfileCommand>, UpdateProfileCommandHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<CustomExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Blog Web API")
               .WithTheme(ScalarTheme.Moon)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
