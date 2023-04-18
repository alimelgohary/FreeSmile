using FreeSmile.ActionFilters;
﻿using FreeSmile.Controllers;
using FreeSmile.DTOs;
using FreeSmile.Models;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Transactions;
using static FreeSmile.Services.AuthHelper;
using static FreeSmile.Services.Helper;

namespace FreeSmile.Services
{
    public class CommonService : ICommonService
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IStringLocalizer<UsersController> _localizer;
        private readonly FreeSmileContext _context;

        public CommonService(ILogger<UsersController> logger, FreeSmileContext context, IStringLocalizer<UsersController> localizer)
        {
            _logger = logger;
            _context = context;
            _localizer = localizer;
        }

        public async Task DeletePost(int id)
        {
            await _context.Database.ExecuteSqlRawAsync($"dbo.DeletePost {id};");
        }
        
        public async Task<bool> CanUsersCommunicateAsync(int user_id, int other_user_id)
        {
            User? user1 = await _context.Users.Include(x => x.Blockers)
                                              .Include(x => x.Blockeds)
                                              .FirstOrDefaultAsync(x => x.Id == user_id);

            bool user1_blocked_user2 = user1?.Blockeds.Where(x => x.Id == other_user_id).Count() > 0;
            bool user1_is_blocked_by_user2 = user1?.Blockers.Where(x => x.Id == other_user_id).Count() > 0;

            return !user1_blocked_user2 && !user1_is_blocked_by_user2;
        }
        
        public async Task<RegularResponse> AddUpdateReviewAsync(ReviewDto value, int user_id)
        {
                var oldReview = await _context.Reviews.FirstOrDefaultAsync(x => x.ReviewerId == user_id);

                if (oldReview is not null)
                {
                    _context.Reviews.Remove(oldReview);
                    await _context.SaveChangesAsync();
                }

                await _context.Reviews.AddAsync(
                    new Review()
                    {
                        Rating = (byte)value.Rating!,
                        Opinion = value.Opinion,
                        ReviewerId = user_id
                    });
                await _context.SaveChangesAsync();

                string message = oldReview is null ? "ReviewAddedSuccessfully" : "ReviewEditedSuccessfully";
                return RegularResponse.Success(message: _localizer[message]);
            }

        public async Task<RegularResponse> DeleteReviewAsync(int user_id)
        {
                var review = await _context.Reviews.FirstOrDefaultAsync(x => x.ReviewerId == user_id);

                if (review is not null)
                {
                    _context.Reviews.Remove(review);
                    await _context.SaveChangesAsync();
                }
                return RegularResponse.Success(message: _localizer["ReviewDeletedSuccessfully"]);
            }

        public async Task<List<GetNotificationDto>> GetNotificationsAsync(int user_id, int page, int size)
        {
                List<Notification> notifications;
                List<GetNotificationDto> actualNotifications = new();

                notifications = await _context.Notifications.Where(x => x.OwnerId == user_id).OrderByDescending(x => x.NotificationId).Skip(size * --page).Take(size).Include(x => x.Temp).ToListAsync();
                foreach (var notification in notifications)
                {
                    actualNotifications.Add(new GetNotificationDto()
                    {
                        NotificationId = notification.NotificationId,
                        HumanizedTime = notification.SentAt.Humanize(culture: CultureInfo.CurrentCulture)!,
                        Seen = (bool)notification.Seen!,
                        Body = string.Format(notification.Temp.Lang(_localizer["lang"]), notification.ActorUsername, notification.PostTitle),
                        NextPage = string.Format(notification.Temp.NextPage, notification.PostId),
                        Icon = notification.Temp.Icon
                    });
                }
                return actualNotifications;
            }

        public async Task NotificationSeenAsync(int notification_id, int user_id_int)
        {
                Notification? notification = await _context.Notifications.FirstOrDefaultAsync(x => x.NotificationId == notification_id && x.OwnerId == user_id_int);
                if (notification is not null)
                {
                    notification.Seen = true;
                    await _context.SaveChangesAsync();
                }
            }

        public async Task<RegularResponse> ReportPostAsync(ReportPostDto value, int user_id)
        {
                PostReport? previousReport = await _context.PostReports.FindAsync(user_id, value.reported_post_id);
                if (previousReport is not null)
                throw new GeneralException(_localizer["AlreadyReportedThisPost"]);

                await _context.PostReports.AddAsync(new()
                {
                    ReporterId = user_id,
                    PostId = (int)value.reported_post_id!,
                    Reason = value.Reason
                });

                await _context.SaveChangesAsync();

                return RegularResponse.Success(message: _localizer["PostReportedSuccessfully"]);
            }

        public async Task<RegularResponse> BlockUserAsync(int user_id, int other_user_id)
        {

                if (user_id.Equals(other_user_id))
                throw new GeneralException(_localizer["CannotBlockYourself"]);

                User? user2 = await _context.Users.FindAsync(other_user_id);

                if (user2 is null)
                throw new GeneralException(_localizer["UserNotFound"]);

                User? user1 = await _context.Users.Include(x => x.Blockeds)
                                                  .FirstOrDefaultAsync(x => x.Id == user_id);

                if (user1!.Blockeds.Where(x => x.Id == other_user_id).Count() > 0)
                throw new GeneralException(_localizer["AlreadyBlockedThisUser"]);

                user1.Blockeds.Add(user2);
                await _context.SaveChangesAsync();

                return RegularResponse.Success(message: _localizer["UserBlockedSuccessfully", user2.Username]);
            }

        public async Task<RegularResponse> UnblockUserAsync(int user_id, int other_user_id)
        {

                if (user_id.Equals(other_user_id))
                throw new GeneralException(_localizer["CannotUnblockYourself"]);

                User? user2 = await _context.Users.FindAsync(other_user_id);

                if (user2 is null)
                throw new GeneralException(_localizer["UserNotFound"]);

                User? user1 = await _context.Users.Include(x => x.Blockeds)
                                                  .FirstOrDefaultAsync(x => x.Id == user_id);

                if (user1!.Blockeds.Where(x => x.Id == other_user_id).Count() == 0)
                throw new GeneralException(_localizer["UserAlreadyUnblocked"]);

                user1.Blockeds.Remove(user2);
                await _context.SaveChangesAsync();

                return RegularResponse.Success(message: _localizer["UserUnblockedSuccessfully", user2.Username]);

            }

        public async Task<List<BlockedUsersDto>> GetBlockedListAsync(int user_id, int page, int size)
        {
            User? user = await _context.Users.Include(x => x.Blockeds)
                                             .FirstOrDefaultAsync(x => x.Id == user_id);

            return user!.Blockeds.Select(x => new BlockedUsersDto()
            {
                Username = x.Username,
                User_Id = x.Id,
                Full_Name = x.Fullname
            }).Skip(size * --page).Take(size).ToList();
        }
        public async Task<RegularResponse> SendMessageAsync(SendMessageDto message, int sender_id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<GetMessageDto>> GetChatHistoryAsync(int user_id, int other_user_id, int page, int size)
        {
            throw new NotImplementedException();
        }

        public async Task<List<RecentMessagesDto>> GetRecentMessagesAsync(int user_id, int page, int size)
        {
            throw new NotImplementedException();
        }
        
        public async Task<byte[]> GetProfilePictureAsync(int user_id, int size)
        {
            if (size < 1 || size > 3)
                size = 1;

            throw new NotImplementedException();
        }
        
        public async Task<RegularResponse> AddUpdateProfilePictureAsync(ProfilePictureDto value, int user_id)
        {
            throw new NotImplementedException();
        }

        public async Task<RegularResponse> DeleteProfilePictureAsync(int user_id)
        {
            throw new NotImplementedException();
        }

        public async Task<RegularResponse> AddCaseAsync(CaseDto value, int user_id)
        {
            throw new NotImplementedException();
        }

        public async Task<RegularResponse> UpdateCaseAsync(UpdateCaseDto value, int user_id)
        {
            throw new NotImplementedException();
        }

        public async Task<RegularResponse> DeleteCaseAsync(int user_id, int case_post_id)
        {
            throw new NotImplementedException();
        }
    }
}
