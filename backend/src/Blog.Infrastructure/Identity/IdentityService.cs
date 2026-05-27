using Microsoft.AspNetCore.Identity;
using Blog.Application.Common.Interfaces;
using Blog.Shared;

namespace Blog.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<Account> _userManager;
    private readonly RoleManager<Role> _roleManager;

    public IdentityService(UserManager<Account> userManager, RoleManager<Role> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
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

        var roleResult = await AssignRoleAsync(id, "Reader");
        if (roleResult.IsFailure)
        {
            return roleResult;
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

    public async Task<Result> AssignRoleAsync(Guid accountId, string roleName)
    {
        var account = await _userManager.FindByIdAsync(accountId.ToString());
        if (account == null)
        {
            return Result.Failure(new Error("Auth.AccountNotFound", "Account not found."));
        }

        var roleExists = await _roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            var newRole = Role.Create(roleName, $"{roleName} Role");
            var createRoleResult = await _roleManager.CreateAsync(newRole);
            if (!createRoleResult.Succeeded)
            {
                var firstError = createRoleResult.Errors.FirstOrDefault()?.Description ?? "Role creation failed.";
                return Result.Failure(new Error("Auth.RoleCreationFailed", firstError));
            }
        }

        var isInRole = await _userManager.IsInRoleAsync(account, roleName);
        if (!isInRole)
        {
            var addToRoleResult = await _userManager.AddToRoleAsync(account, roleName);
            if (!addToRoleResult.Succeeded)
            {
                var firstError = addToRoleResult.Errors.FirstOrDefault()?.Description ?? "Failed to assign role.";
                return Result.Failure(new Error("Auth.RoleAssignmentFailed", firstError));
            }
        }

        return Result.Success();
    }
}
