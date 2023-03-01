using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class Article
    {
        public Article()
        {
            Comments = new HashSet<Comment>();
            Likers = new HashSet<User>();
        }

        public int ArticleId { get; set; }
        public int CatId { get; set; }

        public virtual Post ArticleNavigation { get; set; } = null!;
        public virtual ArticleCat Cat { get; set; } = null!;
        public virtual ICollection<Comment> Comments { get; set; }

        public virtual ICollection<User> Likers { get; set; }
    }
}
