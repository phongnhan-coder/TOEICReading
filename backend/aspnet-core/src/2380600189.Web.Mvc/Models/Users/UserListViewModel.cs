using TOEICReading.Roles.Dto;
using System.Collections.Generic;

namespace TOEICReading.Web.Models.Users;

public class UserListViewModel
{
    public IReadOnlyList<RoleDto> Roles { get; set; }
}
