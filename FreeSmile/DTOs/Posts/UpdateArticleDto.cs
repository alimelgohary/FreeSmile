using FreeSmile.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using FreeSmile.CustomValidations;

namespace FreeSmile.DTOs.Posts
{
    public class UpdateArticleDto : UpdatePostDto
    {
        [DisplayName(nameof(ArticleCategoryId))]
        [Required(ErrorMessage = "required")]
        [ForeignKey(nameof(ArticleCat), "article_cat_id")]
        public int? ArticleCategoryId { get; set; }
    }
}