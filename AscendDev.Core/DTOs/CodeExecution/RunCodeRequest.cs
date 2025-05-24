using System.ComponentModel.DataAnnotations;

namespace AscendDev.Core.DTOs.CodeExecution;

public class RunCodeRequest
{
    [Required(ErrorMessage = "Language is required")]
    public string Language { get; set; } = null!;

    [Required(ErrorMessage = "Code is required")]
    public string Code { get; set; } = null!;
}