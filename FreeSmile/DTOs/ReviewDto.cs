using FreeSmile.CustomValidations;
using FreeSmile.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FreeSmile.DTOs
{
    public class ReviewDto
    {
        [DisplayName(nameof(Opinion))]
        [MaxLength(100, ErrorMessage = "maxchar")]
        public string Opinion { get; set; } = null!;
        
        [DisplayName(nameof(Rating))]
        [Required(ErrorMessage = "required")]
        [Range(1, 5, ErrorMessage = "boundvalue")]
        public byte? Rating { get; set; } = null!;
        
    }
}
