using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class Governate
    {
        public Governate()
        {
            Cases = new HashSet<Case>();
            Listings = new HashSet<Listing>();
            SharingPatients = new HashSet<SharingPatient>();
            Universities = new HashSet<University>();
        }

        public int GovId { get; set; }
        public string NameEn { get; set; } = null!;
        public string NameAr { get; set; } = null!;

        public virtual ICollection<Case> Cases { get; set; }
        public virtual ICollection<Listing> Listings { get; set; }
        public virtual ICollection<SharingPatient> SharingPatients { get; set; }
        public virtual ICollection<University> Universities { get; set; }
    }
}
