using Abp.AutoMapper;
using TOEICReading.Sessions.Dto;

namespace TOEICReading.Web.Views.Shared.Components.TenantChange;

[AutoMapFrom(typeof(GetCurrentLoginInformationsOutput))]
public class TenantChangeViewModel
{
    public TenantLoginInfoDto Tenant { get; set; }
}
