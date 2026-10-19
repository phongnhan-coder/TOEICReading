using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace 2380600189.Controllers
{
    public abstract class 2380600189ControllerBase : AbpController
    {
        protected 2380600189ControllerBase()
        {
            LocalizationSourceName = 2380600189Consts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
