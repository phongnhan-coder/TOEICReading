using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using TOEICReading.Authorization;

namespace TOEICReading;

[DependsOn(
    typeof(TOEICReadingCoreModule),
    typeof(AbpAutoMapperModule))]
public class TOEICReadingApplicationModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.Authorization.Providers.Add<TOEICReadingAuthorizationProvider>();
    }

    public override void Initialize()
    {
        var thisAssembly = typeof(TOEICReadingApplicationModule).GetAssembly();

        IocManager.RegisterAssemblyByConvention(thisAssembly);

        Configuration.Modules.AbpAutoMapper().Configurators.Add(
            // Scan the assembly for classes which inherit from AutoMapper.Profile
            cfg => cfg.AddMaps(thisAssembly)
        );
    }
}
