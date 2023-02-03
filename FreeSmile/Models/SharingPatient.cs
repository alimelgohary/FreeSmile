using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class SharingPatient
    {
        public int SharingId { get; set; }
        public string? PatientPhoneNumber { get; set; }
        public int CaseTypeId { get; set; }
        public int GovernateId { get; set; }

        public virtual CaseType CaseType { get; set; } = null!;
        public virtual Governate Governate { get; set; } = null!;
        public virtual Post Sharing { get; set; } = null!;
    }
}
