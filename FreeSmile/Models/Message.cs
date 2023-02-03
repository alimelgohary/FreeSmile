﻿using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class Message
    {
        public int MessageId { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Body { get; set; } = null!;
        public DateTime SentAt { get; set; }
        public bool Seen { get; set; }

        public virtual User Receiver { get; set; } = null!;
        public virtual User Sender { get; set; } = null!;
    }
}
