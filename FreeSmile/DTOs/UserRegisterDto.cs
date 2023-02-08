using FreeSmile.CustomValidations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FreeSmile.DTOs
{
    [Index(nameof(Username), IsUnique = true)] //test it, also no error message
    public class UserRegisterDto
    {
        [DisplayName(nameof(Username))]
        [Required(ErrorMessage = "required")]
        [RegularExpression("^[A-Za-z]+[A-Za-z0-9_]*[A-Za-z0-9]+$", ErrorMessage = "usernameRegex")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "maxMinChar")]
        public string Username { get; set; } = null!;
        
        
        [Required(ErrorMessage = "required")]
        [DisplayName(nameof(Email))]
        [RegularExpression("", ErrorMessage = "mustBeEmail")] // Provide a real validation, .Net validation sucks
        [MaxLength(100, ErrorMessage = "maxchar")]
        public string Email { get; set; } = null!;
        
        
        [DisplayName(nameof(Password))]
        [Required(ErrorMessage = "required")]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "maxMinchar")]
        public string Password { get; set; } = null!;

        [DisplayName(nameof(Phone))]
        [StringLength(10, ErrorMessage = "exactchar")]
        public string? Phone { get; set; }

        [DisplayName(nameof(Fname))]
        [Required(ErrorMessage = "required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "maxMinchar")]
        public string Fname { get; set; } = null!;

        [DisplayName(nameof(Lname))]
        [Required(ErrorMessage = "required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "maxMinchar")]
        public string Lname { get; set; } = null!;

        [DisplayName(nameof(Gender))]
        [Required(ErrorMessage = "required")]
        public bool Gender { get; set; }

        [DisplayName(nameof(Birthdate))]
        [Age(10, 120, ErrorMessage ="ageminmax")]
        public DateTime? Birthdate { get; set; }
    }
}
