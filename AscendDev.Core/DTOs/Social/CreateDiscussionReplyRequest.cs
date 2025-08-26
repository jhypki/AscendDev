using System.ComponentModel.DataAnnotations;

namespace AscendDev.Core.DTOs.Social;

public class CreateDiscussionReplyRequest
{
    [Required]
    [StringLength(5000, MinimumLength = 5)]
    public string Content { get; set; } = null!;

    public Guid? ParentReplyId { get; set; }
}