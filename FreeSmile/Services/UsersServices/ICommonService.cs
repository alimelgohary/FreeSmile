﻿using FreeSmile.DTOs;
using FreeSmile.Models;
using static FreeSmile.Services.Helper;

namespace FreeSmile.Services
{
    public interface ICommonService
    {
        public Task DeletePost(int id);

        public Task<RegularResponse> AddUpdateReviewAsync(ReviewDto value, int user_id);
        public Task<RegularResponse> DeleteReviewAsync(int user_id);
        public Task<List<GetNotificationDto>> GetNotificationsAsync(int user_id, int page, int size);
        public Task NotificationSeenAsync(int notification_id, int user_id_int);
        public Task<RegularResponse> ReportPostAsync(int post_id, int user_id);
        public Task<RegularResponse> BlockUserAsync(int user_id, int other_user_id);
        public Task<RegularResponse> SendMessageAsync(SendMessageDto message, int user_id, int other_user_id);
        public Task<List<GetMessageDto>> GetChatHistoryAsync(int user_id, int other_user_id, int page, int size);
        public Task<List<RecentMessagesDto>> GetRecentMessagesAsync(int user_id, int page, int size);
        public Task<RegularResponse> AddUpdateProfilePictureAsync(ProfilePictureDto value, int user_id);
        public Task<RegularResponse> DeleteProfilePictureAsync(int user_id);
        
    }
}
