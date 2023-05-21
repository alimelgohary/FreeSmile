using FreeSmile.DTOs.Posts;

namespace FreeSmile.Services
{
    public interface IPublicService
    {
        public Task<GetPostDto> GetPostAsync(int auth_user_id, int postId);
    }
}
