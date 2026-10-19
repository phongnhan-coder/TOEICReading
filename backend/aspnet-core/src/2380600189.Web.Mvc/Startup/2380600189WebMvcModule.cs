using Abp.Modules;
using Abp.Reflection.Extensions;
using TOEICReading.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace TOEICReading.Web.Startup;

[DependsOn(typeof(TOEICReadingWebCoreModule))]
public class TOEICReadingWebMvcModule : AbpModule
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfigurationRoot _appConfiguration;

    public TOEICReadingWebMvcModule(IWebHostEnvironment env)
    {
        _env = env;
        _appConfiguration = env.GetAppConfiguration();
    }

    public override void PreInitialize()
    {
        Configuration.Navigation.Providers.Add<TOEICReadingNavigationProvider>();
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(TOEICReadingWebMvcModule).GetAssembly());
    }
}
