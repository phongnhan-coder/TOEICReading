using TOEICReading.Roles.Dto;
using System.Collections.Generic;

namespace TOEICReading.Web.Models.Common;

public interface IPermissionsEditViewModel
{
    List<FlatPermissionDto> Permissions { get; set; }
}