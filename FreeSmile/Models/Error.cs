using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class Error
    {
        public string TempName { get; set; } = null!;
        public string MessageAr { get; set; } = null!;
        public string MessageEn { get; set; } = null!;
    }
}
