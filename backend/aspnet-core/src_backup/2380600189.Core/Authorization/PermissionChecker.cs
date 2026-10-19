using Abp.Authorization;
using 2380600189.Authorization.Roles;
using 2380600189.Authorization.Users;

namespace 2380600189.Authorization;

public class PermissionChecker : PermissionChecker<Role, User>
{
    public PermissionChecker(UserManager userManager)
        : base(userManager)
    {
    }
}
