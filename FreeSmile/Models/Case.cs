using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class Case
    {
        public int CaseId { get; set; }
        public bool? VisibleContactInfo { get; set; }
        public int CaseTypeId { get; set; }
        public int GovernateId { get; set; }

        public virtual Post CaseNavigation { get; set; } = null!;
        public virtual CaseType CaseType { get; set; } = null!;
        public virtual Governate Governate { get; set; } = null!;
    }
}
