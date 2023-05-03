using DTOs;

namespace FreeSmile.DTOs.Settings
{
    public class GetCommonSettingsDto : GetBasicUserInfo
    {
        public string? Phone { get; set; } = null;
        public string? Email { get; set; } = null;
        public string? Birthdate { get; set; } = null;
        public bool VisibleMail { get; set; }
        public bool VisibleContact { get; set; }
        public bool VisibleBd { get; set; }
        public bool IsOwner { get; set; } = true;
        public virtual void HidePrivate()
        {
            if (!VisibleContact)
                Phone = null;
            if (!VisibleMail)
                Email = null;
            if (!VisibleBd)
                Birthdate = null;

            IsOwner = false;
        }
    }
}