namespace FreeSmile.DTOs.Posts
{
    public class GetListingDto : GetPostDto
    {
        public string Governorate { get; set; } = null!;
        public string ProductCategory { get; set; } = null!;
        public decimal Price { get; set; }
    }
}