using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class Patient
    {
        public int PatientId { get; set; }

        public virtual User PatientNavigation { get; set; } = null!;
    }
}
