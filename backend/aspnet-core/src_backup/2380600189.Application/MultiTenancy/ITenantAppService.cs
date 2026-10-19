using Abp.Application.Services;
using 2380600189.MultiTenancy.Dto;

namespace 2380600189.MultiTenancy;

public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
{
}

