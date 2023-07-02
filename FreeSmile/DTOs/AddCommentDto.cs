using FreeSmile.CustomValidations;
using FreeSmile.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FreeSmile.DTOs
{
    public class AddCommentDto
    {
        [DisplayName(nameof(ArticleId))]
        [Required(ErrorMessage = "required")]
        public int? ArticleId { get; set; }

        [DisplayName(nameof(Body))]
        [Required(ErrorMessage = "required")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "maxMinChar")]
        public string Body { get; set; } = null!;
    }
}