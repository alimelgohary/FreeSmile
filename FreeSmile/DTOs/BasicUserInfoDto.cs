namespace DTOs
{
    public class BasicUserInfoDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string? AcademicDegree { get; set; } = null;
        public byte[]? ProfilePicture { get; set; }
    }
}