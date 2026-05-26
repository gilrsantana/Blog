using Microsoft.AspNetCore.Identity;

namespace Blog.Infrastructure.Identity;

public class AccountRole : IdentityUserRole<Guid>
{
    private AccountRole()
    {
    }

    private AccountRole(Guid accountId, Guid roleId)
    {
        UserId = accountId;
        RoleId = roleId;
    }

    public static AccountRole Create(Guid accountId, Guid roleId)
    {
        return new AccountRole(accountId, roleId);
    }
}
