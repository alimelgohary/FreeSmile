using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class CaseType
    {
        public CaseType()
        {
            Cases = new HashSet<Case>();
            SharingPatients = new HashSet<SharingPatient>();
        }

        public int CaseTypeId { get; set; }
        public string NameEn { get; set; } = null!;
        public string NameAr { get; set; } = null!;

        public virtual ICollection<Case> Cases { get; set; }
        public virtual ICollection<SharingPatient> SharingPatients { get; set; }
    }
}
