using DTOs;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FreeSmile.DTOs.Admins
{
    public class RejectReasonDto
    {
        [DisplayName(nameof(rejectReason))]
        [Required(ErrorMessage = "required")]
        [Range(1, 3, ErrorMessage = "boundvalue")]
        public int? rejectReason { get; set; }

    }
}