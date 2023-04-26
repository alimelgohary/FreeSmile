namespace FreeSmile.DTOs
{
    public class GetBlockedUsersDto
    {
        public int User_Id { get; set; }
        public string Full_Name { get; set; } = null!;
        public string Username { get; set; } = null!;
    }
}