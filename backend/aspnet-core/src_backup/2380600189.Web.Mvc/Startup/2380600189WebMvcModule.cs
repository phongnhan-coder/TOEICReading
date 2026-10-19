using Abp.Modules;
using Abp.Reflection.Extensions;
using 2380600189.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace 2380600189.Web.Startup;

[DependsOn(typeof(2380600189WebCoreModule))]
public class 2380600189WebMvcModule : AbpModule
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfigurationRoot _appConfiguration;

    public 2380600189WebMvcModule(IWebHostEnvironment env)
    {
        _env = env;
        _appConfiguration = env.GetAppConfiguration();
    }

    public override void PreInitialize()
    {
        Configuration.Navigation.Providers.Add<2380600189NavigationProvider>();
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(2380600189WebMvcModule).GetAssembly());
    }
}
