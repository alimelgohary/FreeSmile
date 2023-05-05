using DTOs;

namespace FreeSmile.DTOs.Posts
{
    public class GetPostDto
    {
        public GetBasicUserInfo UserInfo { get; set; } = null!;
        public GetBasicDentistInfo? DentistInfo { get; set; } = null;
        public int PostId { get; set; }
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public string TimeWritten { get; set; } = null!;
        public string? TimeUpdated { get; set; } = null;
        public List<byte[]>? Images { get; set; } = null;
        public string? Phone { get; set; } = null;
    }
}