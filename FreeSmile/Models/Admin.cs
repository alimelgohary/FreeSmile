using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class Admin
    {
        public int AdminId { get; set; }

        public virtual User AdminNavigation { get; set; } = null!;
    }
}
