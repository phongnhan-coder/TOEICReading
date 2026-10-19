using Abp.Application.Services;
using 2380600189.Authorization.Accounts.Dto;
using System.Threading.Tasks;

namespace 2380600189.Authorization.Accounts;

public interface IAccountAppService : IApplicationService
{
    Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

    Task<RegisterOutput> Register(RegisterInput input);
}
