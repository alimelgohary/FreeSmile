using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class Review
    {
        public int ReviewId { get; set; }
        public int ReviewerId { get; set; }
        public byte Rating { get; set; }
        public string? Opinion { get; set; }

        public virtual User Reviewer { get; set; } = null!;
    }
}
