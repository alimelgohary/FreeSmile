using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class Notification
    {
        public int NotificationId { get; set; }
        public int OwnerId { get; set; }
        public DateTime? SentAt { get; set; }
        public bool? Seen { get; set; }
        public string? PostTitle { get; set; }
        public string? ActorUsername { get; set; }
        public int TempId { get; set; }

        public virtual User Owner { get; set; } = null!;
        public virtual NotificationTemplate Temp { get; set; } = null!;
    }
}
