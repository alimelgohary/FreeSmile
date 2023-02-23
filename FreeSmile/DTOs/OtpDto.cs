using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FreeSmile.DTOs
{
    public class OtpDto
    {
        [DisplayName(nameof(Otp))]
        [Required(ErrorMessage = "required")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "mustBeNumber")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "exactchar")]
        public string Otp { get; set; } = null!;
    }
}