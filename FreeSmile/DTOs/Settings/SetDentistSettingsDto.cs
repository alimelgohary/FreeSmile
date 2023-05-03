using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FreeSmile.DTOs.Settings
{
    public class SetDentistSettingsDto : SetCommonSettingsDto
    {
        [DisplayName(nameof(Bio))]
        [MaxLength(300, ErrorMessage = "maxchar")]
        public string? Bio { get; set; } = null;


        [DisplayName(nameof(FbUsername))]
        [MaxLength(50, ErrorMessage = "maxchar")]
        public string? FbUsername { get; set; } = null;


        [DisplayName(nameof(LinkedUsername))]
        [MaxLength(50, ErrorMessage = "maxchar")]
        public string? LinkedUsername { get; set; } = null;


        [DisplayName(nameof(GScholarUsername))]
        [MaxLength(50, ErrorMessage = "maxchar")]
        public string? GScholarUsername { get; set; } = null;


        [DisplayName(nameof(ResearchGateUsername))]
        [MaxLength(50, ErrorMessage = "maxchar")]
        public string? ResearchGateUsername { get; set; } = null;
    }
}