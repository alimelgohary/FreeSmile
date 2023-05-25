using FreeSmile.DTOs;
using FreeSmile.DTOs.Posts;
using FreeSmile.DTOs.Settings;
using static FreeSmile.Services.AuthHelper;
using static FreeSmile.Services.Helper;

namespace FreeSmile.Services
{
    public interface ICommonService
    {
        public Task<Role> GetCurrentRole(int user_id);
        public Task DeletePostDangerousAsync(int id);
        public Task<bool> CanUsersCommunicateAsync(int user_id, int other_user_id);
        public Task<IEnumerable<int>> GetUserEnemiesAsync(int user_id);
        public Task<ReviewDto> GetReviewAsync(int user_id);
        public Task<RegularResponse> AddUpdateReviewAsync(ReviewDto value, int user_id);
        public Task<RegularResponse> DeleteReviewAsync(int user_id);
        public Task<List<GetNotificationDto>> GetNotificationsAsync(int user_id, int page, int size);
        public Task NotificationSeenAsync(int notification_id, int user_id_int);
        public Task<RegularResponse> ReportPostAsync(ReportPostDto value, int user_id);
        public Task<RegularResponse> BlockUserAsync(int user_id, int other_user_id);
        public Task<RegularResponse> UnblockUserAsync(int user_id, int other_user_id);
        public Task<List<GetBlockedUsersDto>> GetBlockedListAsync(int user_id, int page, int size);
        public Task<GetMessageDto> SendMessageAsync(SendMessageDto message, int user_id);
        public Task<List<GetMessageDto>> GetChatHistoryAsync(int user_id, int other_user_id, int page, int size, int after);
        public Task<List<RecentMessagesDto>> GetRecentMessagesAsync(int user_id, int page, int size, string? q);
        public Task<byte[]?> GetProfilePictureAsync(int auth_user_id, int other_user_id, byte size);
        public Task<byte[]?> GetProfilePictureDangerousAsync(int user_id, byte size = 1);
        public Task<byte[]> AddUpdateProfilePictureAsync(AddProfilePictureDto value, int user_id);
        public RegularResponse DeleteProfilePictureAsync(int user_id);
        public Task<int> AddCaseAsync(AddCaseDto value, int user_id, string roleString);
        public Task<int> AddPostAsync(AddPostDto value, int user_id);
        public Task<List<byte[]>?> GetPostImagesAsync(int postId);
        public Task<RegularResponse> UpdateCaseAsync(UpdateCaseDto value, int user_id, string roleString);
        public Task<RegularResponse> DeletePostAsync(int user_id, int case_post_id, string roleString);
        public Task<GetCommonSettingsDto> GetCommonSettingsAsync(int user_id);
        public Task<GetCommonSettingsDto> UpdateCommonSettingsAsync(SetCommonSettingsDto settings, int user_id);
        public Task AddNotificationDangerousAsync(int owner_id, NotificationTemplates temp_name, string? post_title = null, int? post_id = null, string? actor_username = null, int? likes = null, int? comments = null);

    }
}
