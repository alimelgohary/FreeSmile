using FreeSmile.DTOs;
using static FreeSmile.Services.Helper;

namespace FreeSmile.Services
{
    public interface ICommonService
    {
        public Task DeletePost(int id);

        public Task<RegularResponse> AddReviewAsync(ReviewDto value, int user_id);
        public Task<RegularResponse> DeleteReviewAsync(int user_id);
        
        // TODO: GET: Recent Messages
        // TODO: GET: Get messages history with someone
        // TODO: POST: send a message
        // TOOD: PUT: seen request
        // TODO: PUT: Block user
        // TODO: POST: Report post
        // TODO: GET: Notifications
    }
}
