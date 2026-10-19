using Abp.Application.Services;
using TOEICReading.MultiTenancy.Dto;

namespace TOEICReading.MultiTenancy;

public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
{
}

