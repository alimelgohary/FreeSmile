using FreeSmile.CustomValidations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FreeSmile.DTOs
{
    public class DeleteMyAccountDto
    {
        [DisplayName(nameof(CurrentPassword))]
        [Required(ErrorMessage = "required")]
        public string CurrentPassword { get; set; } = null!;
    }
}