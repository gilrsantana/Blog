using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Blog.Application.Common.Interfaces;
using Blog.Shared;

namespace Blog.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<Account> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly JwtSettings _jwtSettings;

    public IdentityService(
        UserManager<Account> userManager,
        RoleManager<Role> roleManager,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtSettings = jwtSettings.Value;
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

        var accessToken = await GenerateAccessTokenAsync(account);
        var refreshToken = GenerateRefreshToken();

        account.UpdateRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
        await _userManager.UpdateAsync(account);

        return Result.Success(new TokenResponse(accessToken, refreshToken));
    }

    public async Task<Result<TokenResponse>> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        var principal = GetPrincipalFromExpiredToken(accessToken);
        if (principal == null)
        {
            return Result.Failure<TokenResponse>(new Error("Auth.InvalidToken", "Invalid access token."));
        }

        var userIdString = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userIdString))
        {
            return Result.Failure<TokenResponse>(new Error("Auth.InvalidToken", "Invalid access token claims."));
        }

        var account = await _userManager.FindByIdAsync(userIdString);
        if (account == null)
        {
            return Result.Failure<TokenResponse>(new Error("Auth.AccountNotFound", "Account not found."));
        }

        if (account.RefreshToken != refreshToken || account.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return Result.Failure<TokenResponse>(new Error("Auth.InvalidRefreshToken", "Invalid or expired refresh token."));
        }

        var newAccessToken = await GenerateAccessTokenAsync(account);
        var newRefreshToken = GenerateRefreshToken();

        account.UpdateRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));
        await _userManager.UpdateAsync(account);

        return Result.Success(new TokenResponse(newAccessToken, newRefreshToken));
    }

    private async Task<string> GenerateAccessTokenAsync(Account account)
    {
        var roles = await _userManager.GetRolesAsync(account);
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, account.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, account.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            return principal;
        }
        catch
        {
            return null;
        }
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
