namespace FreeSmile.DTOs.Settings
{
    public class GetDentistSettingsDto : GetCommonSettingsDto
    {
        public string AcademicDegree { get; set; } = null!;
        public string University { get; set; } = null!;
        public string? Bio { get; set; } = null;
        public string? FbUsername { get; set; } = null;
        public string? LinkedUsername { get; set; } = null;
        public string? GScholarUsername { get; set; } = null;
        public string? ResearchGateUsername { get; set; } = null;
        public bool HasPendingVerificationRequest { get; set; }

        public override void HidePrivate()
        {
            base.HidePrivate();
            HasPendingVerificationRequest = false;
        }

    }
}