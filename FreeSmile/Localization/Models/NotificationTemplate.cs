using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class NotificationTemplate
    {
        public string Lang(string language)
        {
            if (language.ToLower() == "ar")
            {
                return BodyAr;
            }
            else
            {
                return BodyEn;
            }
        }
    }
}
