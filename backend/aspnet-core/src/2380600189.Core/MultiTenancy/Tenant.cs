using Abp.MultiTenancy;
using TOEICReading.Authorization.Users;

namespace TOEICReading.MultiTenancy;

public class Tenant : AbpTenant<User>
{
    public Tenant()
    {
    }

    public Tenant(string tenancyName, string name)
        : base(tenancyName, name)
    {
    }
}
