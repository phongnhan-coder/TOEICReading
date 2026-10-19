using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using 2380600189.Authorization;
using 2380600189.Controllers;
using 2380600189.Users;
using 2380600189.Web.Models.Users;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace 2380600189.Web.Controllers;

[AbpMvcAuthorize(PermissionNames.Pages_Users)]
public class UsersController : 2380600189ControllerBase
{
    private readonly IUserAppService _userAppService;

    public UsersController(IUserAppService userAppService)
    {
        _userAppService = userAppService;
    }

    public async Task<ActionResult> Index()
    {
        var roles = (await _userAppService.GetRoles()).Items;
        var model = new UserListViewModel
        {
            Roles = roles
        };
        return View(model);
    }

    public async Task<ActionResult> EditModal(long userId)
    {
        var user = await _userAppService.GetAsync(new EntityDto<long>(userId));
        var roles = (await _userAppService.GetRoles()).Items;
        var model = new EditUserModalViewModel
        {
            User = user,
            Roles = roles
        };
        return PartialView("_EditModal", model);
    }

    public ActionResult ChangePassword()
    {
        return View();
    }
}
