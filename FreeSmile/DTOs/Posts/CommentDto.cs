using DTOs;

namespace FreeSmile.DTOs.Posts
{
    public class CommentDto
    {
        public int CommentId { get; set; }
        public string Body { get; set; } = null!;
        public string TimeWritten { get; set; } = null!;
        public DateTime Written { get; set; }
        public GetBasicUserInfo UserInfo { get; set; } = null!;
        public GetBasicDentistInfo? DentistInfo { get; set; } = null;
    }
}