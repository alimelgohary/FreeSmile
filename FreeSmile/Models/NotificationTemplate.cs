﻿using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class NotificationTemplate
    {
        public NotificationTemplate()
        {
            Notifications = new HashSet<Notification>();
        }

        public int TempId { get; set; }
        public string TempName { get; set; } = null!;
        public string BodyAr { get; set; } = null!;
        public string BodyEn { get; set; } = null!;
        public string NextPage { get; set; } = null!;
        public string? Icon { get; set; }

        public virtual ICollection<Notification> Notifications { get; set; }
    }
}
