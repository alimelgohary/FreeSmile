using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class Dentist
    {
        public Dentist()
        {
            Portfolios = new HashSet<Portfolio>();
        }

        public int DentistId { get; set; }
        public string Name2 { get; set; } = null!;
        public string Name3 { get; set; } = null!;
        public string Name4 { get; set; } = null!;
        public string? Bio { get; set; }
        public string? FbUsername { get; set; }
        public string? LinkedUsername { get; set; }
        public string? GScholarUsername { get; set; }
        public int CurrentDegree { get; set; }
        public int CurrentUniversity { get; set; }

        public virtual AcademicDegree CurrentDegreeNavigation { get; set; } = null!;
        public virtual University CurrentUniversityNavigation { get; set; } = null!;
        public virtual User DentistNavigation { get; set; } = null!;
        public virtual VerificationRequest VerificationRequest { get; set; } = null!;
        public virtual ICollection<Portfolio> Portfolios { get; set; }
    }
}
