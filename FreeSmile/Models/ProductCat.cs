using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class ProductCat
    {
        public ProductCat()
        {
            Listings = new HashSet<Listing>();
        }

        public int ProductCatId { get; set; }
        public string NameEn { get; set; } = null!;
        public string NameAr { get; set; } = null!;

        public virtual ICollection<Listing> Listings { get; set; }
    }
}
