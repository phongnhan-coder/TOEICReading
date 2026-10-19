using Abp.Authorization;
using TOEICReading.Authorization.Roles;
using TOEICReading.Authorization.Users;

namespace TOEICReading.Authorization;

public class PermissionChecker : PermissionChecker<Role, User>
{
    public PermissionChecker(UserManager userManager)
        : base(userManager)
    {
    }
}
