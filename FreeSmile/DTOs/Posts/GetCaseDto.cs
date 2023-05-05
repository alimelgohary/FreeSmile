namespace FreeSmile.DTOs.Posts
{
    public class GetCaseDto : GetPostDto
    {
        public string Governorate { get; set; } = null!;
        public string CaseType { get; set; } = null!;

    }
}