using FreeSmile.CustomValidations;
using FreeSmile.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FreeSmile.DTOs
{
    public class RequestOtpDto
    {
        [DisplayName(nameof(UsernameOrEmail))]
        [MaxLength(100, ErrorMessage = "maxchar")]
        [OrRegex("^[A-Za-z]+[A-Za-z0-9_]*[A-Za-z0-9]+$", "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$")]
        public string UsernameOrEmail { get; set; } = null!;
        
    }
}
