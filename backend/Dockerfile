# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Copy everything and restore
COPY . .
RUN dotnet restore backend/Blog.slnx

# Publish the presentation project
RUN dotnet publish backend/src/Blog.Presentation/Blog.Presentation.csproj -c Release -o /app --no-restore

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Blog.Presentation.dll"]
