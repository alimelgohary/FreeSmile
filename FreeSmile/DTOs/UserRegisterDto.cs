using FreeSmile.CustomValidations;
using FreeSmile.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FreeSmile.DTOs
{
    public class UserRegisterDto
    {
        [DisplayName(nameof(Username))]
        [Required(ErrorMessage = "required")]
        [RegularExpression("^[A-Za-z]+[A-Za-z0-9_]*[A-Za-z0-9]+$", ErrorMessage = "usernameRegex")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "maxMinChar")]
        [Unique(nameof(User), nameof(Username))]
        public string Username { get; set; } = null!;
        
        
        [Required(ErrorMessage = "required")]
        [DisplayName(nameof(Email))]
        [RegularExpression("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$", ErrorMessage = "invalidvalue")] // Provide a real validation, .Net validation sucks, .edu.eg
        [MaxLength(100, ErrorMessage = "maxchar")]
        [UniqueVerified(nameof(User), nameof(Email))]
        public string Email { get; set; } = null!;
        
        
        [DisplayName(nameof(Password))]
        [Required(ErrorMessage = "required")]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "maxMinchar")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()\-+=:\[\]?\._;])[A-Za-z\d\s!@#$%^&*()\-+=:\[\]?\._;~`']{4,}$", ErrorMessage = "passwordregex")]
        public string Password { get; set; } = null!;

        [DisplayName(nameof(Phone))]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "exactchar")]
        [RegularExpression("^1[0-9]*$", ErrorMessage = "mustBeNumber")]
        public string? Phone { get; set; }

        [DisplayName(nameof(Fullname))]
        [Required(ErrorMessage = "required")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "maxMinchar")]
        public string Fullname { get; set; } = null!;

        [DisplayName(nameof(Gender))]
        [Required(ErrorMessage = "required")]
        public bool Gender { get; set; }

        [DisplayName(nameof(Birthdate))]
        [Age(5, 120, ErrorMessage ="ageminmax")]
        [ValidDate()]
        public string? Birthdate { get; set; }
    }
}
