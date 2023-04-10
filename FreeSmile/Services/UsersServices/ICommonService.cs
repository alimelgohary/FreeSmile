using FreeSmile.DTOs;
using static FreeSmile.Services.Helper;

namespace FreeSmile.Services
{
    public interface ICommonService
    {
        public Task DeletePost(int id);

        public Task<RegularResponse> AddReviewAsync(ReviewDto value, int user_id);
        public Task<RegularResponse> DeleteReviewAsync(int user_id);
        public Task<List<GetNotificationDto>> GetNotificationsAsync(int user_id, int page, int size);
        public Task NotificationSeenAsync(int notification_id, int user_id_int);


        // TODO: GET: AddUpdate profile picture       // TODO: delete profile picture
        // TODO: GET: Recent Messages
        // TODO: GET: Get messages history with someone
        // TODO: POST: send a message
        // TODO: PUT: Block user
        // TODO: POST: Report post
    }
}
