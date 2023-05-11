namespace FreeSmile.DTOs.Admins
{
    public class LogSummaryDto
    {
        public string? Error { get; set; }
        public DateTime Time { get; set; }
        public string Humanized { get; set; } = null!;
    }
}