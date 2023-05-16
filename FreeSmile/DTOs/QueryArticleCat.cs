using FreeSmile.CustomValidations;
using FreeSmile.Models;
using System.ComponentModel;

namespace FreeSmile.DTOs
{
    public class QueryArticleCat
    {
        [DisplayName(nameof(ArticleCategoryId))]
        [ForeignKey(nameof(ArticleCat), "article_cat_id", bypassZero: true)]
        public int ArticleCategoryId { get; set; }
    }
}