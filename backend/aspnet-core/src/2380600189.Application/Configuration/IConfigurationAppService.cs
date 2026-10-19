using TOEICReading.Configuration.Dto;
using System.Threading.Tasks;

namespace TOEICReading.Configuration;

public interface IConfigurationAppService
{
    Task ChangeUiTheme(ChangeUiThemeInput input);
}
