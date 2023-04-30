using System.Security.Claims;
using Blog.Models;

namespace Blog.Extensions;

public static class RoleClaimsExtension
{
    public static IEnumerable<Claim> GetClaims(this User user)
    {
        var result = new List<Claim>
        {
            new(ClaimTypes.Name, user.Email)
        };

        if (user.Roles is not null)
        {
            result.AddRange(
                user.Roles.Select(role => new Claim(ClaimTypes.Role, role.Slug)));
        }
        

        // var roles = user.Roles;
        // if (roles is not null)
        // {
        //     foreach (var role in roles)
        //     {
        //         result.Add(new Claim(ClaimTypes.Role, role.Slug));
        //     }
        // }
            

        return result;
    }
}