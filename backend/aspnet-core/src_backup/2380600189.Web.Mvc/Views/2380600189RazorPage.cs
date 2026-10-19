using Abp.AspNetCore.Mvc.Views;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Mvc.Razor.Internal;

namespace 2380600189.Web.Views;

public abstract class 2380600189RazorPage<TModel> : AbpRazorPage<TModel>
{
    [RazorInject]
    public IAbpSession AbpSession { get; set; }

    protected 2380600189RazorPage()
    {
        LocalizationSourceName = 2380600189Consts.LocalizationSourceName;
    }
}
