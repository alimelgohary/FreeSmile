using FreeSmile.CustomValidations;
using FreeSmile.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FreeSmile.DTOs
{
    public class SetCommonSettingsDto
    {
        [DisplayName(nameof(Username))]
        [RegularExpression("^[A-Za-z]+[A-Za-z0-9_]*[A-Za-z0-9]+$", ErrorMessage = "usernameRegex")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "maxMinChar")]
        [Unique(nameof(User), nameof(Username))]
        public string? Username { get; set; }


        [DisplayName(nameof(Fullname))]
        [Required(ErrorMessage = "required")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "maxMinchar")]
        public string Fullname { get; set; } = null!;


        [DisplayName(nameof(Phone))]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "exactchar")]
        [RegularExpression("^1[0-9]*$", ErrorMessage = "mustBeNumber")]
        public string? Phone { get; set; } = null;


        [DisplayName(nameof(Birthdate))]
        [Age(5, 120, ErrorMessage = "ageminmax")]
        [ValidDate()]
        public string? Birthdate { get; set; }


        public bool VisibleMail { get; set; }
        
        
        public bool VisibleContact { get; set; }


        public bool VisibleBd { get; set; }
    }
}