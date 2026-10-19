using Abp.AspNetCore.Mvc.Authorization;
using TOEICReading.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace TOEICReading.Web.Controllers;

[AbpMvcAuthorize]
public class HomeController : TOEICReadingControllerBase
{
    public ActionResult Index()
    {
        return View();
    }
}
