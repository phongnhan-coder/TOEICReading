using Abp.Modules;
using Abp.Reflection.Extensions;
using TOEICReading.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace TOEICReading.Web.Host.Startup
{
    [DependsOn(
       typeof(TOEICReadingWebCoreModule))]
    public class TOEICReadingWebHostModule : AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public TOEICReadingWebHostModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(TOEICReadingWebHostModule).GetAssembly());
        }
    }
}
