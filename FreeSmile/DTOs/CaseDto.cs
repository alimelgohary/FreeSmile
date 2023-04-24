using FreeSmile.CustomValidations;
using FreeSmile.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FreeSmile.DTOs
{
    public class CaseDto : PostDto
    {
        [DisplayName(nameof(GovernorateId))]
        [ForeignKey(nameof(Governate), "gov_id", bypassZero: true, ErrorMessage = "invalidchoice")]
        public int GovernorateId { get; set; }
        
        [DisplayName(nameof(CaseTypeId))]
        [Required(ErrorMessage = "required")]
        [ForeignKey(nameof(CaseType), "case_type_id", ErrorMessage = "invalidchoice")]
        public int? CaseTypeId { get; set; }
    }
}