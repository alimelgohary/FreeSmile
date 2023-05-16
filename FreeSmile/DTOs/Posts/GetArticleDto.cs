namespace FreeSmile.DTOs.Posts
{
    public class GetArticleDto : GetPostDto
    {
        public string ArticleCateogry{ get; set; } = null!;
        public int Likes{ get; set; }
        public int Comments{ get; set; }
    }
}