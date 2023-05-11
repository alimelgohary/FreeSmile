namespace FreeSmile.DTOs.Admins
{
    public class LogDto
    {
        public DateTime DateTimeUTC { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public string? Level { get; set; }

        public string? MessageTemplate { get; set; }

        public Properties? Properties { get; set; }
    }

    public partial class Properties
    {
        public string? SourceContext { get; set; }

        public Guid ActionId { get; set; }

        public string? ActionName { get; set; }

        public string? RequestId { get; set; }

        public string? RequestPath { get; set; }

        public string? ConnectionId { get; set; }
    }
}

