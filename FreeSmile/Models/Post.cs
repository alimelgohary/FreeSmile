using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class Post
    {
        public Post()
        {
            PostReports = new HashSet<PostReport>();
        }

        public int PostId { get; set; }
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public DateTime? TimeWritten { get; set; }
        public DateTime? TimeUpdated { get; set; }
        public int WriterId { get; set; }

        public virtual User Writer { get; set; } = null!;
        public virtual Article Article { get; set; } = null!;
        public virtual Case Case { get; set; } = null!;
        public virtual Listing Listing { get; set; } = null!;
        public virtual SharingPatient SharingPatient { get; set; } = null!;
        public virtual ICollection<PostReport> PostReports { get; set; }
    }
}
