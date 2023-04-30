namespace FreeSmile.DTOs
{
    public class SetDentistSettingsDto : SetCommonSettingsDto
    {
        public string? Bio { get; set; } = null;
        public string? FbUsername { get; set; } = null;
        public string? LinkedUsername { get; set; } = null;
        public string? GScholarUsername { get; set; } = null;
        public string? ResearchGateUsername { get; set; } = null;
    }
}