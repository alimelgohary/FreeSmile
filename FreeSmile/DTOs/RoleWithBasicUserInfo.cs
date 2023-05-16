namespace DTOs
{
    public class RoleWithBasicUserInfo : GetBasicUserInfo
    {
        public string Role { get; set; } = null!;
    }
}