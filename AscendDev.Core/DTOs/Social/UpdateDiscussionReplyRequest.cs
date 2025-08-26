using System.ComponentModel.DataAnnotations;

namespace AscendDev.Core.DTOs.Social;

public class UpdateDiscussionReplyRequest
{
    [StringLength(5000, MinimumLength = 5)]
    public string? Content { get; set; }

    public bool? IsSolution { get; set; }
}