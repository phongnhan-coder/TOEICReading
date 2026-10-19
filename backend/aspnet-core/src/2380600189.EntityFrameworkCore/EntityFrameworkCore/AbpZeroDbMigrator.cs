using Abp.Domain.Uow;
using Abp.EntityFrameworkCore;
using Abp.MultiTenancy;
using Abp.Zero.EntityFrameworkCore;

namespace TOEICReading.EntityFrameworkCore;

public class AbpZeroDbMigrator : AbpZeroDbMigrator<TOEICReadingDbContext>
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
