using Abp.AspNetCore.Mvc.Authorization;
using TOEICReading.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace TOEICReading.Web.Controllers;

[AbpMvcAuthorize]
public class AboutController : TOEICReadingControllerBase
{
    public ActionResult Index()
    {
        return View();
    }
}
