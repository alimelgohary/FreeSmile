using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class PostReport
    {
        public int ReporterId { get; set; }
        public int PostId { get; set; }
        public string? Reason { get; set; }

        public virtual Post Post { get; set; } = null!;
        public virtual User Reporter { get; set; } = null!;
    }
}
