using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class Listing
    {
        public int ListingId { get; set; }
        public decimal Price { get; set; }
        public int GovernateId { get; set; }
        public int CatId { get; set; }

        public virtual ProductCat Cat { get; set; } = null!;
        public virtual Governate Governate { get; set; } = null!;
        public virtual Post ListingNavigation { get; set; } = null!;
    }
}
