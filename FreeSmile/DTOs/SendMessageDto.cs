using FreeSmile.CustomValidations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using FreeSmile.Models;
using System.Text.Json.Serialization;

namespace FreeSmile.DTOs
{
    public class SendMessageDto
    {
        [DisplayName(nameof(Receiver_Id))]
        [Required(ErrorMessage = "required")]
        [ForeignKey(nameof(User), "id")]
        public int? Receiver_Id { get; set; }

        [Required(ErrorMessage = "required")]
        [DisplayName(nameof(Message))]
        [MaxLength(300, ErrorMessage = "maxchar")]
        public string Message { get; set; } = null!;
    }
}