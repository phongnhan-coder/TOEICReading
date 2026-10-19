using TOEICReading.Roles.Dto;
using System.Collections.Generic;

namespace TOEICReading.Web.Models.Roles;

public class RoleListViewModel
{
    public IReadOnlyList<PermissionDto> Permissions { get; set; }
}
