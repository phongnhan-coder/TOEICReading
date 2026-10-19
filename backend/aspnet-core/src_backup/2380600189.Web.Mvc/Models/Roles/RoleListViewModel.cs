using 2380600189.Roles.Dto;
using System.Collections.Generic;

namespace 2380600189.Web.Models.Roles;

public class RoleListViewModel
{
    public IReadOnlyList<PermissionDto> Permissions { get; set; }
}
