using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class SuperAdmin
    {
        public int SuperAdminId { get; set; }

        public virtual User SuperAdminNavigation { get; set; } = null!;
    }
}
