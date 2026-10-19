using Abp.AspNetCore.Mvc.Views;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Mvc.Razor.Internal;

namespace TOEICReading.Web.Views;

public abstract class TOEICReadingRazorPage<TModel> : AbpRazorPage<TModel>
{
    [RazorInject]
    public IAbpSession AbpSession { get; set; }

    protected TOEICReadingRazorPage()
    {
        LocalizationSourceName = TOEICReadingConsts.LocalizationSourceName;
    }
}
