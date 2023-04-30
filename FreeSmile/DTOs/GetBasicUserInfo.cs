namespace DTOs
{
    public class GetBasicUserInfo
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public byte[]? ProfilePicture { get; set; }
    }
}