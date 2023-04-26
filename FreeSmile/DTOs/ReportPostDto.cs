using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using FreeSmile.Models;
using FreeSmile.CustomValidations;

namespace FreeSmile.DTOs
{
    public class ReportPostDto
    {
        [DisplayName(nameof(reported_post_id))]
        [Required(ErrorMessage = "required")]
        [ForeignKey(nameof(Post), "post_id")]
        public int? reported_post_id { get; set; }

        [DisplayName(nameof(Reason))]
        [MaxLength(100, ErrorMessage = "maxchar")]
        public string? Reason { get; set; }
    }
}