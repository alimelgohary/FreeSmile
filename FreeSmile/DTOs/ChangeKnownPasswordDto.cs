using FreeSmile.CustomValidations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FreeSmile.DTOs
{
    public class ChangeKnownPasswordDto
    {
        [DisplayName(nameof(CurrentPassword))]
        [Required(ErrorMessage = "required")]
        public string CurrentPassword { get; set; } = null!;

        [StringLength(50, MinimumLength = 10, ErrorMessage = "maxMinchar")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()\-+=:\[\]?\._;])[A-Za-z\d\s!@#$%^&*()\-+=:\[\]?\._;~`']{4,}$", ErrorMessage = "passwordregex")]
        [DisplayName(nameof(NewPassword))]
        public string NewPassword { get; set; } = null!;
    }
}