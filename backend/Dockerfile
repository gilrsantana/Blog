# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Copy project files first to restore dependencies
COPY src/Blog.Shared/Blog.Shared.csproj src/Blog.Shared/
COPY src/Blog.Domain/Blog.Domain.csproj src/Blog.Domain/
COPY src/Blog.Application/Blog.Application.csproj src/Blog.Application/
COPY src/Blog.Infrastructure/Blog.Infrastructure.csproj src/Blog.Infrastructure/
COPY src/Blog.Presentation/Blog.Presentation.csproj src/Blog.Presentation/

# Restore dependencies
RUN dotnet restore src/Blog.Presentation/Blog.Presentation.csproj

# Copy all source files
COPY src/ src/

# Build and publish the application
WORKDIR /source/src/Blog.Presentation
RUN dotnet publish -c Release -o /app --no-restore

# Run Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./

# Expose port (standard ASP.NET Core port)
EXPOSE 8080

ENTRYPOINT ["dotnet", "Blog.Presentation.dll"]
