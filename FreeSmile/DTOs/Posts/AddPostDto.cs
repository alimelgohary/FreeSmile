using FreeSmile.CustomValidations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FreeSmile.DTOs.Posts
{
    public class AddPostDto
    {
        [DisplayName(nameof(Title))]
        [Required(ErrorMessage = "required")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "maxMinChar")]
        public string Title { get; set; } = null!;

        [DisplayName(nameof(Body))]
        [Required(ErrorMessage = "required")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "maxMinChar")]
        public string Body { get; set; } = null!;

        [DisplayName(nameof(Images))]
        [MaxFileSize(5, ErrorMessage = "TooLarge")]
        [AllowedExtensions(new[] { ".jpg", ".jpeg", ".png", ".webp", ".bmp", ".gif" }, ErrorMessage = "ImagesOnly")]
        public IFormFileCollection? Images { get; set; } = null;
    }
}