using FreeSmile.CustomValidations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FreeSmile.DTOs.Auth
{
    public class ResetPasswordDto : OtpDto
    {
        [Required(ErrorMessage = "required")]
        [DisplayName(nameof(UsernameOrEmail))]
        [MaxLength(100, ErrorMessage = "maxchar")]
        [OrRegex("^[A-Za-z]+[A-Za-z0-9_]*[A-Za-z0-9]+$", "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$")]
        public string UsernameOrEmail { get; set; } = null!;

        [StringLength(50, MinimumLength = 10, ErrorMessage = "maxMinchar")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()\-+=:\[\]?\._;])[A-Za-z\d\s!@#$%^&*()\-+=:\[\]?\._;~`']{4,}$", ErrorMessage = "passwordregex")]
        [DisplayName(nameof(NewPassword))]
        [Required(ErrorMessage = "required")]
        public string NewPassword { get; set; } = null!;

    }
}