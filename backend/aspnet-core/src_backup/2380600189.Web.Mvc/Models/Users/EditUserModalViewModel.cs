using 2380600189.Roles.Dto;
using 2380600189.Users.Dto;
using System.Collections.Generic;
using System.Linq;

namespace 2380600189.Web.Models.Users;

public class EditUserModalViewModel
{
    public UserDto User { get; set; }

    public IReadOnlyList<RoleDto> Roles { get; set; }

    public bool UserIsInRole(RoleDto role)
    {
        return User.RoleNames != null && User.RoleNames.Any(r => r == role.NormalizedName);
    }
}
