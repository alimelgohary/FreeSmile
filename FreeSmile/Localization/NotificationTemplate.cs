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
                return TitleAr;
            }
            else
            {
                return TitleEn;
            }
        }
        public string LangBody(string language)
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
