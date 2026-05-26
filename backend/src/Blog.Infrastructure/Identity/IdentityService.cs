using Microsoft.AspNetCore.Identity;
using Blog.Application.Common.Interfaces;
using Blog.Shared;

namespace Blog.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<Account> _userManager;

    public IdentityService(UserManager<Account> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<TokenResponse>> LoginAsync(string email, string password)
    {
        var account = await _userManager.FindByEmailAsync(email);
        if (account == null)
        {
            return Result.Failure<TokenResponse>(new Error("Auth.InvalidCredentials", "Invalid credentials."));
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(account, password);
        if (!isPasswordValid)
        {
            return Result.Failure<TokenResponse>(new Error("Auth.InvalidCredentials", "Invalid credentials."));
        }

        return Result.Success(new TokenResponse("dummy-access-token", account.RefreshToken));
    }

    public async Task<Result<TokenResponse>> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        return Result.Success(new TokenResponse("dummy-new-access-token", "dummy-new-refresh-token"));
    }

    public async Task<Result> RegisterAsync(Guid id, string email, string password)
    {
        var account = Account.Create(id, email);
        var result = await _userManager.CreateAsync(account, password);
        if (!result.Succeeded)
        {
            var firstError = result.Errors.FirstOrDefault()?.Description ?? "Registration failed.";
            return Result.Failure(new Error("Auth.RegistrationFailed", firstError));
        }
        return Result.Success();
    }

    public async Task<Result> UpdatePasswordAsync(Guid accountId, string currentPassword, string newPassword)
    {
        var account = await _userManager.FindByIdAsync(accountId.ToString());
        if (account == null) return Result.Failure(new Error("Auth.AccountNotFound", "Account not found."));

        var result = await _userManager.ChangePasswordAsync(account, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            var firstError = result.Errors.FirstOrDefault()?.Description ?? "Password update failed.";
            return Result.Failure(new Error("Auth.PasswordUpdateFailed", firstError));
        }
        return Result.Success();
    }

    public async Task<Result> DeleteAccountAsync(Guid accountId)
    {
        var account = await _userManager.FindByIdAsync(accountId.ToString());
        if (account == null) return Result.Failure(new Error("Auth.AccountNotFound", "Account not found."));

        var result = await _userManager.DeleteAsync(account);
        if (!result.Succeeded)
        {
            var firstError = result.Errors.FirstOrDefault()?.Description ?? "Account deletion failed.";
            return Result.Failure(new Error("Auth.DeletionFailed", firstError));
        }
        return Result.Success();
    }

    public async Task<Result> InactivateAccountAsync(Guid accountId)
    {
        var account = await _userManager.FindByIdAsync(accountId.ToString());
        if (account == null) return Result.Failure(new Error("Auth.AccountNotFound", "Account not found."));

        account.UpdateLockoutStatus(blocked: true);
        var result = await _userManager.UpdateAsync(account);
        if (!result.Succeeded)
        {
            return Result.Failure(new Error("Auth.InactivationFailed", "Inactivation failed."));
        }
        return Result.Success();
    }
}
