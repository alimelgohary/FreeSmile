using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class PostImage
    {
        public int PostId { get; set; }
        public string ImageName { get; set; } = null!;

        public virtual Post Post { get; set; } = null!;
    }
}
