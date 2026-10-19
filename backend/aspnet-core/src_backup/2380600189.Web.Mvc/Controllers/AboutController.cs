using Abp.AspNetCore.Mvc.Authorization;
using 2380600189.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace 2380600189.Web.Controllers;

[AbpMvcAuthorize]
public class AboutController : 2380600189ControllerBase
{
    public ActionResult Index()
    {
        return View();
    }
}
