using FreeSmile.CustomValidations;
using FreeSmile.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FreeSmile.DTOs.Posts
{
    public class AddListingDto : AddPostDto
    {
        [DisplayName(nameof(GovernorateId))]
        [ForeignKey(nameof(Governate), "gov_id", bypassZero: true)]
        public int GovernorateId { get; set; }

        [DisplayName(nameof(ListingCategoryId))]
        [Required(ErrorMessage = "required")]
        [ForeignKey(nameof(ProductCat), "product_cat_id")]
        public int? ListingCategoryId { get; set; }

        [Required(ErrorMessage = "required")]
        [DisplayName(nameof(Price))]
        public decimal? Price { get; set; }
    }
}