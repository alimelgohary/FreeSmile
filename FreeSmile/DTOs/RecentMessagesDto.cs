using DTOs;

namespace FreeSmile.DTOs
{
    public class RecentMessagesDto : BasicUserInfoDto
    {
        public string LastMessage { get; set; } = null!;
        public string LastMessageTime { get; set; } = null!;
        public bool IsSender { get; set; }
        public bool Seen { get; set; }
        public bool IsAvailable { get; set; }
    }
}