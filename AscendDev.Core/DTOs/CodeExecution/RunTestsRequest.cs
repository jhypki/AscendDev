using System.ComponentModel.DataAnnotations;

namespace AscendDev.Core.DTOs.CodeExecution;

public class RunTestsRequest
{
    [Required(ErrorMessage = "Lesson ID is required")]
    public string LessonId { get; set; } = null!;

    [Required(ErrorMessage = "Code is required")]
    public string Code { get; set; } = null!;
}