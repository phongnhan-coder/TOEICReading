using Abp.Application.Services;
using 2380600189.Sessions.Dto;
using System.Threading.Tasks;

namespace 2380600189.Sessions;

public interface ISessionAppService : IApplicationService
{
    Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
}
