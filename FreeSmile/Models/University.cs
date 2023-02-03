using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class University
    {
        public University()
        {
            Dentists = new HashSet<Dentist>();
        }

        public int UniversityId { get; set; }
        public string NameEn { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public int GovId { get; set; }

        public virtual Governate Gov { get; set; } = null!;
        public virtual ICollection<Dentist> Dentists { get; set; }
    }
}
