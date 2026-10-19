using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace TOEICReading.Controllers
{
    public abstract class TOEICReadingControllerBase : AbpController
    {
        protected TOEICReadingControllerBase()
        {
            LocalizationSourceName = TOEICReadingConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
