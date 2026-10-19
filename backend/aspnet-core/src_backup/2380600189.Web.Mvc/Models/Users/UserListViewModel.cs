using 2380600189.Roles.Dto;
using System.Collections.Generic;

namespace 2380600189.Web.Models.Users;

public class UserListViewModel
{
    public IReadOnlyList<RoleDto> Roles { get; set; }
}
