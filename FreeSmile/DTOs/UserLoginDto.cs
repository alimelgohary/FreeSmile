using FreeSmile.CustomValidations;
using FreeSmile.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FreeSmile.DTOs
{
    public class UserLoginDto
    {
        [Required(ErrorMessage = "required")]
        [DisplayName(nameof(UsernameOrEmail))]
        [MaxLength(100, ErrorMessage = "maxchar")]
        [OrRegex("^[A-Za-z]+[A-Za-z0-9_]*[A-Za-z0-9]+$", "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$", ErrorMessage = "mustbeemail")]
        public string UsernameOrEmail { get; set; } = null!;

        [DisplayName(nameof(Password))]
        [Required(ErrorMessage = "required")]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "maxMinchar")]
        public string Password { get; set; } = null!;
    }
}
