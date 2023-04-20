using FreeSmile.CustomValidations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FreeSmile.DTOs
{
    public class ProfilePictureDto
    {
        [DisplayName(nameof(ProfilePicture))]
        [Required(ErrorMessage = "required")]
        [MaxFileSize(3, ErrorMessage = "TooLarge")]
        [AllowedExtensions(new[] { ".jpg", ".jpeg", ".png", ".webp", ".bmp", ".gif"}, ErrorMessage = "ImagesOnly")]
        public IFormFile ProfilePicture { get; set; } = null!;
    }
}