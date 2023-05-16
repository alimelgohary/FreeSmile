using FreeSmile.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using FreeSmile.CustomValidations;

namespace FreeSmile.DTOs.Posts
{
    public class UpdateSharingDto : UpdatePostDto
    {
        [DisplayName(nameof(GovernorateId))]
        [ForeignKey(nameof(Governate), "gov_id", bypassZero: true)]
        public int GovernorateId { get; set; }

        [DisplayName(nameof(CaseTypeId))]
        [Required(ErrorMessage = "required")]
        [ForeignKey(nameof(CaseType), "case_type_id")]
        public int? CaseTypeId { get; set; }

        [DisplayName(nameof(PatientPhone))]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "exactchar")]
        [RegularExpression("^1[0-9]*$", ErrorMessage = "mustBeNumber")]
        public string? PatientPhone { get; set; } = null;
    }
}