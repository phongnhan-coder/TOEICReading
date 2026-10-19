using Abp.Authorization;
using Abp.Runtime.Session;
using TOEICReading.Configuration.Dto;
using System.Threading.Tasks;

namespace TOEICReading.Configuration;

[AbpAuthorize]
public class ConfigurationAppService : TOEICReadingAppServiceBase, IConfigurationAppService
{
    public async Task ChangeUiTheme(ChangeUiThemeInput input)
    {
        await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
    }
}
