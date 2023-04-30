using DTOs;

namespace FreeSmile.DTOs
{
    public class GetCommonSettingsDto : GetBasicUserInfo
    {
        public string? Phone { get; set; } = null;
        public string Email { get; set; } = null!;
        public string? Birthdate { get; set; } = null;
        public bool VisibleMail{ get; set; }
        public bool VisibleContact { get; set; }
    }
}