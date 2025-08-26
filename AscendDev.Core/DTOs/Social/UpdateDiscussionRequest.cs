using System.ComponentModel.DataAnnotations;

namespace AscendDev.Core.DTOs.Social;

public class UpdateDiscussionRequest
{
    [StringLength(200, MinimumLength = 5)]
    public string? Title { get; set; }

    [StringLength(5000, MinimumLength = 10)]
    public string? Content { get; set; }

    public bool? IsPinned { get; set; }

    public bool? IsLocked { get; set; }
}