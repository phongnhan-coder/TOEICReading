using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Uow;
using Abp.MultiTenancy;
using 2380600189.EntityFrameworkCore.Seed.Host;
using 2380600189.EntityFrameworkCore.Seed.Tenants;
using Microsoft.EntityFrameworkCore;
using System;
using System.Transactions;

namespace 2380600189.EntityFrameworkCore.Seed;

public static class SeedHelper
{
    public static void SeedHostDb(IIocResolver iocResolver)
    {
        WithDbContext<2380600189DbContext>(iocResolver, SeedHostDb);
    }

    public static void SeedHostDb(2380600189DbContext context)
    {
        context.SuppressAutoSetTenantId = true;

        // Host seed
        new InitialHostDbBuilder(context).Create();

        // Default tenant seed (in host database).
        new DefaultTenantBuilder(context).Create();
        new TenantRoleAndUserBuilder(context, 1).Create();
    }

    private static void WithDbContext<TDbContext>(IIocResolver iocResolver, Action<TDbContext> contextAction)
        where TDbContext : DbContext
    {
        using (var uowManager = iocResolver.ResolveAsDisposable<IUnitOfWorkManager>())
        {
            using (var uow = uowManager.Object.Begin(TransactionScopeOption.Suppress))
            {
                var context = uowManager.Object.Current.GetDbContext<TDbContext>(MultiTenancySides.Host);

                contextAction(context);

                uow.Complete();
            }
        }
    }
}
