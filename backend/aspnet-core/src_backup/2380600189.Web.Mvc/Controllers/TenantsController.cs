using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using 2380600189.Authorization;
using 2380600189.Controllers;
using 2380600189.MultiTenancy;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace 2380600189.Web.Controllers;

[AbpMvcAuthorize(PermissionNames.Pages_Tenants)]
public class TenantsController : 2380600189ControllerBase
{
    private readonly ITenantAppService _tenantAppService;

    public TenantsController(ITenantAppService tenantAppService)
    {
        _tenantAppService = tenantAppService;
    }

    public ActionResult Index() => View();

    public async Task<ActionResult> EditModal(int tenantId)
    {
        var tenantDto = await _tenantAppService.GetAsync(new EntityDto(tenantId));
        return PartialView("_EditModal", tenantDto);
    }
}
