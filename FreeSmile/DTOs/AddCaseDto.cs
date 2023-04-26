using FreeSmile.CustomValidations;
using FreeSmile.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FreeSmile.DTOs
{
    public class AddCaseDto : AddPostDto
    {
        [DisplayName(nameof(GovernorateId))]
        [ForeignKey(nameof(Governate), "gov_id", bypassZero: true)]
        public int GovernorateId { get; set; }
        
        [DisplayName(nameof(CaseTypeId))]
        [Required(ErrorMessage = "required")]
        [ForeignKey(nameof(CaseType), "case_type_id")]
        public int? CaseTypeId { get; set; }
    }
}