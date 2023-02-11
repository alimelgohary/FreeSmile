using System;
using System.Collections.Generic;

namespace FreeSmile.Models
{
    public partial class User
    {
        public User()
        {
            MessageReceivers = new HashSet<Message>();
            MessageSenders = new HashSet<Message>();
            Notifications = new HashSet<Notification>();
            PostReports = new HashSet<PostReport>();
            Posts = new HashSet<Post>();
            Reviews = new HashSet<Review>();
            Blockeds = new HashSet<User>();
            Blockers = new HashSet<User>();
        }

        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Salt { get; set; } = null!;
        public bool IsVerified { get; set; }
        public string? Phone { get; set; }
        public string Fullname { get; set; } = null!;
        public bool Gender { get; set; }
        public DateTime? Bd { get; set; }
        public int? Age { get; set; }
        public bool VisibleMail { get; set; }
        public bool VisibleContact { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Otp { get; set; }
        public DateTime? OtpExp { get; set; }

        public virtual Admin Admin { get; set; } = null!;
        public virtual Dentist Dentist { get; set; } = null!;
        public virtual Patient Patient { get; set; } = null!;
        public virtual ICollection<Message> MessageReceivers { get; set; }
        public virtual ICollection<Message> MessageSenders { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<PostReport> PostReports { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }

        public virtual ICollection<User> Blockeds { get; set; }
        public virtual ICollection<User> Blockers { get; set; }
    }
}
