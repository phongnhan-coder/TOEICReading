using System.ComponentModel.DataAnnotations;

namespace 2380600189.Configuration.Dto;

public class ChangeUiThemeInput
{
    [Required]
    [StringLength(32)]
    public string Theme { get; set; }
}
