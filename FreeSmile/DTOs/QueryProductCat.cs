using FreeSmile.Models;
using System.ComponentModel;
using FreeSmile.CustomValidations;

namespace FreeSmile.DTOs
{
    public class QueryProductCat
    {
        [DisplayName(nameof(ListingCategoryId))]
        [ForeignKey(nameof(ProductCat), "product_cat_id", bypassZero: true)]
        public int ListingCategoryId { get; set; }
    }
}