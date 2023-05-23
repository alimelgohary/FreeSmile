using DTOs;
using FreeSmile.DTOs.Posts;

namespace FreeSmile.Services
{
    public interface IPublicService
    {
        public Task<GetPostDto> GetPostAsync(int auth_user_id, int postId);
        public Task<List<CommentDto>> GetArticleCommentsAsync(int auth_user_id, int size, int[] previouslyFetched, int articleId);
        public Task<List<GetBasicUserInfo>> GetArticleLikesAsync(int auth_user_id, int size, int[] previouslyFetched, int articleId);
    }
}
