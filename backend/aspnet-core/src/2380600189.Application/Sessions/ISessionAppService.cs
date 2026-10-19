using Abp.Application.Services;
using TOEICReading.Sessions.Dto;
using System.Threading.Tasks;

namespace TOEICReading.Sessions;

public interface ISessionAppService : IApplicationService
{
    Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
}
