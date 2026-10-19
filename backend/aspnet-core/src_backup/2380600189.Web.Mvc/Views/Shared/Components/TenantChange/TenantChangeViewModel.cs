using Abp.AutoMapper;
using 2380600189.Sessions.Dto;

namespace 2380600189.Web.Views.Shared.Components.TenantChange;

[AutoMapFrom(typeof(GetCurrentLoginInformationsOutput))]
public class TenantChangeViewModel
{
    public TenantLoginInfoDto Tenant { get; set; }
}
