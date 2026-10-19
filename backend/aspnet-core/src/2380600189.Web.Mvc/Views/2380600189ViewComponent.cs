using Abp.AspNetCore.Mvc.ViewComponents;

namespace TOEICReading.Web.Views;

public abstract class TOEICReadingViewComponent : AbpViewComponent
{
    protected TOEICReadingViewComponent()
    {
        LocalizationSourceName = TOEICReadingConsts.LocalizationSourceName;
    }
}
