using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class AcademicDegree
    {
        public AcademicDegree()
        {
            Dentists = new HashSet<Dentist>();
            VerificationRequests = new HashSet<VerificationRequest>();
        }

        public int DegId { get; set; }
        public string NameEn { get; set; } = null!;
        public string NameAr { get; set; } = null!;

        public virtual ICollection<Dentist> Dentists { get; set; }
        public virtual ICollection<VerificationRequest> VerificationRequests { get; set; }
    }
}
