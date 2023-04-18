using FreeSmile.CustomValidations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using FreeSmile.Models;
using System.Text.Json.Serialization;

namespace FreeSmile.DTOs
{
    public class GetMessageDto
    {
        public int MessageId { get; set; }
        public bool IsSender { get; set; }
        public string Message { get; set; } = null!;
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public bool Seen { get; set; }
        public string SentAt { get; set; } = null!;
    }
}