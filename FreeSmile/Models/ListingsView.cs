using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class ListingsView
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public int PostId { get; set; }
        public string AcademicDegreeEn { get; set; } = null!;
        public string AcademicDegreeAr { get; set; } = null!;
        public string CurrentUnivrsityEn { get; set; } = null!;
        public string CurrentUnivrsityAr { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public DateTime? TimeWritten { get; set; }
        public DateTime? TimeUpdated { get; set; }
        public string? Phone { get; set; }
        public decimal Price { get; set; }
        public int ProductCatId { get; set; }
        public string ProductCatEn { get; set; } = null!;
        public string ProductCatAr { get; set; } = null!;
        public int GovId { get; set; }
        public string GovNameEn { get; set; } = null!;
        public string GovNameAr { get; set; } = null!;
    }
}
