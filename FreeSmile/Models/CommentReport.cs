using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class CommentReport
    {
        public int ReporterId { get; set; }
        public int CommentId { get; set; }
        public string? Reason { get; set; }

        public virtual Comment Comment { get; set; } = null!;
        public virtual User Reporter { get; set; } = null!;
    }
}
