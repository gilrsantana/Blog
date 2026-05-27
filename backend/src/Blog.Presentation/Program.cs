using Blog.Presentation.Configurations;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddPresentationServices(builder.Configuration);

var app = builder.Build();

app.Configure();

app.Run();

// Expose the implicitly defined Program class to the integration test project
public partial class Program { }
