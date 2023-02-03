using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class ArticleCat
    {
        public ArticleCat()
        {
            Articles = new HashSet<Article>();
        }

        public int ArticleCatId { get; set; }
        public string NameEn { get; set; } = null!;
        public string NameAr { get; set; } = null!;

        public virtual ICollection<Article> Articles { get; set; }
    }
}
