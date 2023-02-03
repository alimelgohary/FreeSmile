using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class Portfolio
    {
        public int DentistId { get; set; }
        public int PortfolioId { get; set; }
        public string? BeforeImage { get; set; }
        public string AfterImage { get; set; } = null!;
        public string? CaseDescription { get; set; }

        public virtual Dentist Dentist { get; set; } = null!;
    }
}
