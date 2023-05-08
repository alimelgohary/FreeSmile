using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class DentistHomeView
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public int PostId { get; set; }
        public string? AcademicDegreeEn { get; set; }
        public string? AcademicDegreeAr { get; set; }
        public string? CurrentUnivrsityEn { get; set; }
        public string? CurrentUnivrsityAr { get; set; }
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public DateTime? TimeWritten { get; set; }
        public DateTime? TimeUpdated { get; set; }
        public string? Phone { get; set; }
        public int CaseTypeId { get; set; }
        public string CaseTypeEn { get; set; } = null!;
        public string CaseTypeAr { get; set; } = null!;
        public int GovId { get; set; }
        public string GovNameEn { get; set; } = null!;
        public string GovNameAr { get; set; } = null!;
    }
}
