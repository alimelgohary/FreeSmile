using FreeSmile.CustomValidations;
using FreeSmile.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FreeSmile.DTOs
{
    public class VerificationDto
    {
        [DisplayName(nameof(UniversityRequested))]
        [Required(ErrorMessage = "required")]
        [ForeignKey(nameof(University), "university_id", ErrorMessage = "invalidchoice")]
        public int? UniversityRequested { get; set; }

        [DisplayName(nameof(DegreeRequested))]
        [Required(ErrorMessage = "required")]
        [ForeignKey(nameof(AcademicDegree), "deg_id", ErrorMessage = "invalidchoice")]
        public int? DegreeRequested { get; set; }

        [DisplayName(nameof(NatIdPhoto))]
        [Required(ErrorMessage = "required")]
        [MaxFileSize(5, ErrorMessage = "TooLarge")]
        [AllowedExtensions(new[] {".jpg", ".jpeg", ".png", ".jpe", ".jfif", ".bmp" }, ErrorMessage = "imagesonly")]
        public IFormFile NatIdPhoto { get; set; } = null!;

        [DisplayName(nameof(ProofOfDegreePhoto))]
        [Required(ErrorMessage = "required")]
        [MaxFileSize(5, ErrorMessage = "TooLarge")]
        [AllowedExtensions(new[] {".jpg", ".jpeg", ".png", ".jpe", ".jfif", ".bmp"}, ErrorMessage = "imagesonly")]
        public IFormFile ProofOfDegreePhoto { get; set; } = null!;

    }
}
