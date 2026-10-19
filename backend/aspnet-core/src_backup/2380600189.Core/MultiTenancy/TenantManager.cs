using Abp.Application.Features;
using Abp.Domain.Repositories;
using Abp.MultiTenancy;
using 2380600189.Authorization.Users;
using 2380600189.Editions;

namespace 2380600189.MultiTenancy;

public class TenantManager : AbpTenantManager<Tenant, User>
{
    public TenantManager(
        IRepository<Tenant> tenantRepository,
        IRepository<TenantFeatureSetting, long> tenantFeatureRepository,
        EditionManager editionManager,
        IAbpZeroFeatureValueStore featureValueStore)
        : base(
            tenantRepository,
            tenantFeatureRepository,
            editionManager,
            featureValueStore)
    {
    }
}
