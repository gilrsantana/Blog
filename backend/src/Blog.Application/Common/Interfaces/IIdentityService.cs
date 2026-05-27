using Blog.Shared;

namespace Blog.Application.Common.Interfaces;

public record TokenResponse(string AccessToken, string RefreshToken);

public interface IIdentityService
{
    Task<Result<TokenResponse>> LoginAsync(string email, string password);
    Task<Result<TokenResponse>> RefreshTokenAsync(string accessToken, string refreshToken);
    Task<Result> RegisterAsync(Guid id, string email, string password);
    Task<Result> UpdatePasswordAsync(Guid accountId, string currentPassword, string newPassword);
    Task<Result> DeleteAccountAsync(Guid accountId);
    Task<Result> InactivateAccountAsync(Guid accountId);
    Task<Result> AssignRoleAsync(Guid accountId, string roleName);
}
