using 2380600189.Roles.Dto;
using System.Collections.Generic;

namespace 2380600189.Web.Models.Common;

public interface IPermissionsEditViewModel
{
    List<FlatPermissionDto> Permissions { get; set; }
}