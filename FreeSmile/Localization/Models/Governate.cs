using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class Governate
    {
        public string Lang(string language)
        {
            if (language.ToLower() == "ar")
            {
                return NameAr;
            }
            else
            {
                return NameEn;
            }
        }
    }
}
