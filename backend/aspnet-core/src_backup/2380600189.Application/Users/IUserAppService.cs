using Abp.Application.Services;
using Abp.Application.Services.Dto;
using 2380600189.Roles.Dto;
using 2380600189.Users.Dto;
using System.Threading.Tasks;

namespace 2380600189.Users;

public interface IUserAppService : IAsyncCrudAppService<UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>
{
    Task DeActivate(EntityDto<long> user);
    Task Activate(EntityDto<long> user);
    Task<ListResultDto<RoleDto>> GetRoles();
    Task ChangeLanguage(ChangeUserLanguageDto input);

    Task<bool> ChangePassword(ChangePasswordDto input);
}
