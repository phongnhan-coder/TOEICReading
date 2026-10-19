using Abp.AspNetCore.Mvc.ViewComponents;

namespace 2380600189.Web.Views;

public abstract class 2380600189ViewComponent : AbpViewComponent
{
    protected 2380600189ViewComponent()
    {
        LocalizationSourceName = 2380600189Consts.LocalizationSourceName;
    }
}
