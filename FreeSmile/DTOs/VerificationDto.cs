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
        [Required]
        [DisplayName("university")]
        [ForeignKey(nameof(University), "university_id", ErrorMessage = "invalidchoice")]
        public int CurrentUniversity { get; set; }

        [Required]
        [DisplayName("degree")]
        [ForeignKey(nameof(AcademicDegree), "deg_id", ErrorMessage = "invalidchoice")]
        public int DegreeRequested { get; set; }

        
        [MaxFileSize(2, ErrorMessage = "TooLarge")]
        [AllowedExtensions(new[] {".jpg", ".jpeg", ".png", ".jpe", ".jfif", ".bmp" , ".pdf"}, ErrorMessage = "imagespdfonly")]
        public IFormFile NatIdPhoto { get; set; } = null!;


        [MaxFileSize(2, ErrorMessage = "TooLarge")]
        [AllowedExtensions(new[] {".jpg", ".jpeg", ".png", ".jpe", ".jfif", ".bmp" , ".pdf"}, ErrorMessage = "imagespdfonly")]
        public IFormFile ProofOfDegreePhoto { get; set; } = null!;

    }
}
