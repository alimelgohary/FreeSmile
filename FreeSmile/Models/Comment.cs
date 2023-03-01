using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class Comment
    {
        public Comment()
        {
            CommentReports = new HashSet<CommentReport>();
        }

        public int CommentId { get; set; }
        public string Body { get; set; } = null!;
        public DateTime? TimeWritten { get; set; }
        public int WriterId { get; set; }
        public int ArticleId { get; set; }

        public virtual Article Article { get; set; } = null!;
        public virtual User Writer { get; set; } = null!;
        public virtual ICollection<CommentReport> CommentReports { get; set; }
    }
}
