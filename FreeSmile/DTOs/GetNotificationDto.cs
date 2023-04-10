using FreeSmile.CustomValidations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using FreeSmile.Models;
using System.Text.Json.Serialization;

namespace FreeSmile.DTOs
{
    public class GetNotificationDto
    {
        public int NotificationId { get; set; }
        [JsonPropertyName("Time")]
        public string HumanizedTime { get; set; } = null!;
        public bool Seen { get; set; }
        public string Body { get; set; } = null!;
        public string NextPage { get; set; } = null!;
        public string? Icon { get; set; }
    }
}