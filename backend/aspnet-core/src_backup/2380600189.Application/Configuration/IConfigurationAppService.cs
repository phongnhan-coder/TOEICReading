using 2380600189.Configuration.Dto;
using System.Threading.Tasks;

namespace 2380600189.Configuration;

public interface IConfigurationAppService
{
    Task ChangeUiTheme(ChangeUiThemeInput input);
}
