using Abp.AutoMapper;
using 2380600189.Roles.Dto;
using 2380600189.Web.Models.Common;

namespace 2380600189.Web.Models.Roles;

[AutoMapFrom(typeof(GetRoleForEditOutput))]
public class EditRoleModalViewModel : GetRoleForEditOutput, IPermissionsEditViewModel
{
    public bool HasPermission(FlatPermissionDto permission)
    {
        return GrantedPermissionNames.Contains(permission.Name);
    }
}
