using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using TOEICReading.EntityFrameworkCore;
using TOEICReading.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace TOEICReading.Web.Tests;

[DependsOn(
    typeof(TOEICReadingWebMvcModule),
    typeof(AbpAspNetCoreTestBaseModule)
)]
public class TOEICReadingWebTestModule : AbpModule
{
    public TOEICReadingWebTestModule(TOEICReadingEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
    }

    public override void PreInitialize()
    {
        Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(TOEICReadingWebTestModule).GetAssembly());
    }

    public override void PostInitialize()
    {
        IocManager.Resolve<ApplicationPartManager>()
            .AddApplicationPartsIfNotAddedBefore(typeof(TOEICReadingWebMvcModule).Assembly);
    }
}