using FreeSmile.CustomValidations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using FreeSmile.Models;

namespace FreeSmile.DTOs
{
    public class UpdatePostDto
    {
        [DisplayName(nameof(updated_post_id))]
        [Required(ErrorMessage = "required")]
        [ForeignKey(nameof(Post), "post_id")]
        public int? updated_post_id { get; set; }

        [DisplayName(nameof(Title))]
        [Required(ErrorMessage = "required")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "maxMinChar")]
        public string Title { get; set; } = null!;

        [DisplayName(nameof(Body))]
        [Required(ErrorMessage = "required")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "maxMinChar")]
        public string Body { get; set; } = null!;
    }
}