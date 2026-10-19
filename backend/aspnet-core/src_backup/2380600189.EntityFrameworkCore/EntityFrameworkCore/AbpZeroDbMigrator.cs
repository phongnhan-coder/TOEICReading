using Abp.Domain.Uow;
using Abp.EntityFrameworkCore;
using Abp.MultiTenancy;
using Abp.Zero.EntityFrameworkCore;

namespace 2380600189.EntityFrameworkCore;

public class AbpZeroDbMigrator : AbpZeroDbMigrator<2380600189DbContext>
{
    public AbpZeroDbMigrator(
        IUnitOfWorkManager unitOfWorkManager,
        IDbPerTenantConnectionStringResolver connectionStringResolver,
        IDbContextResolver dbContextResolver)
        : base(
            unitOfWorkManager,
            connectionStringResolver,
            dbContextResolver)
    {
    }
}
