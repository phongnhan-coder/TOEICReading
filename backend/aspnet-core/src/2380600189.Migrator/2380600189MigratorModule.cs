using Abp.Events.Bus;
using Abp.Modules;
using Abp.Reflection.Extensions;
using TOEICReading.Configuration;
using TOEICReading.EntityFrameworkCore;
using TOEICReading.Migrator.DependencyInjection;
using Castle.MicroKernel.Registration;
using Microsoft.Extensions.Configuration;

namespace TOEICReading.Migrator;

[DependsOn(typeof(TOEICReadingEntityFrameworkModule))]
public class TOEICReadingMigratorModule : AbpModule
{
    private readonly IConfigurationRoot _appConfiguration;

    public TOEICReadingMigratorModule(TOEICReadingEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbSeed = true;

        _appConfiguration = AppConfigurations.Get(
            typeof(TOEICReadingMigratorModule).GetAssembly().GetDirectoryPathOrNull()
        );
    }

    public override void PreInitialize()
    {
        Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(
            TOEICReadingConsts.ConnectionStringName
        );

        Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
        Configuration.ReplaceService(
            typeof(IEventBus),
            () => IocManager.IocContainer.Register(
                Component.For<IEventBus>().Instance(NullEventBus.Instance)
            )
        );
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(TOEICReadingMigratorModule).GetAssembly());
        ServiceCollectionRegistrar.Register(IocManager);
    }
}
