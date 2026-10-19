using Abp.Dependency;
using 2380600189.Identity;
using Castle.Windsor.MsDependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace 2380600189.Migrator.DependencyInjection;

public static class ServiceCollectionRegistrar
{
    public static void Register(IIocManager iocManager)
    {
        var services = new ServiceCollection();

        IdentityRegistrar.Register(services);

        WindsorRegistrationHelper.CreateServiceProvider(iocManager.IocContainer, services);
    }
}
