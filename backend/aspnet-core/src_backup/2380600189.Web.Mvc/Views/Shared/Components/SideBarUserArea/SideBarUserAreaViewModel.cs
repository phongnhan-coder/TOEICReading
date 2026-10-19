using 2380600189.Sessions.Dto;

namespace 2380600189.Web.Views.Shared.Components.SideBarUserArea;

public class SideBarUserAreaViewModel
{
    public GetCurrentLoginInformationsOutput LoginInformations { get; set; }

    public bool IsMultiTenancyEnabled { get; set; }

    public string GetShownLoginName()
    {
        var userName = LoginInformations.User.UserName;

        if (!IsMultiTenancyEnabled)
        {
            return userName;
        }

        return LoginInformations.Tenant == null
            ? ".\\" + userName
            : LoginInformations.Tenant.TenancyName + "\\" + userName;
    }
}
