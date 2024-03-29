﻿using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class VerificationRequest
    {
        public int OwnerId { get; set; }
        public int DegreeRequested { get; set; }
        public int UniversityRequested { get; set; }

        public virtual AcademicDegree DegreeRequestedNavigation { get; set; } = null!;
        public virtual Dentist Owner { get; set; } = null!;
        public virtual University UniversityRequestedNavigation { get; set; } = null!;
    }
}
