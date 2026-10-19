using Abp.Zero.EntityFrameworkCore;
using 2380600189.Authorization.Roles;
using 2380600189.Authorization.Users;
using 2380600189.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace 2380600189.EntityFrameworkCore;

public class 2380600189DbContext : AbpZeroDbContext<Tenant, Role, User, 2380600189DbContext>
{
    /* Define a DbSet for each entity of the application */

    public 2380600189DbContext(DbContextOptions<2380600189DbContext> options)
        : base(options)
    {
    }
}
