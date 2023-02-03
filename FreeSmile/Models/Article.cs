using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class Article
    {
        public int ArticleId { get; set; }
        public int CatId { get; set; }

        public virtual Post ArticleNavigation { get; set; } = null!;
        public virtual ArticleCat Cat { get; set; } = null!;
    }
}
