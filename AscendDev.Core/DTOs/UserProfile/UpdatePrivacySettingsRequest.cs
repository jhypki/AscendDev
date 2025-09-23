using System.ComponentModel.DataAnnotations;

namespace AscendDev.Core.DTOs.UserProfile
{
    public class UpdatePrivacySettingsRequest
    {
        [Required]
        public bool IsProfilePublic { get; set; }

        [Required]
        public bool ShowEmail { get; set; }

        [Required]
        public bool ShowProgress { get; set; }

        [Required]
        public bool ShowAchievements { get; set; }
    }
}