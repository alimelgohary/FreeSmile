﻿using FreeSmile.CustomValidations;
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
        [Unique(nameof(User), nameof(Username), ErrorMessage = "unique")]
        public string Username { get; set; } = null!;
        
        
        [Required(ErrorMessage = "required")]
        [DisplayName(nameof(Email))]
        [RegularExpression("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$", ErrorMessage = "mustBeEmail")] // Provide a real validation, .Net validation sucks, .edu.eg
        [MaxLength(100, ErrorMessage = "maxchar")]
        [Unique(nameof(User), nameof(Email), ErrorMessage = "unique")]
        public string Email { get; set; } = null!;
        
        
        [DisplayName(nameof(Password))]
        [Required(ErrorMessage = "required")]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "maxMinchar")]
        public string Password { get; set; } = null!;

        [DisplayName(nameof(Phone))]
        [StringLength(10, ErrorMessage = "exactchar")]
        [Unique(nameof(User), nameof(Phone), ErrorMessage = "unique")]
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
