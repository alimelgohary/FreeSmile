using FreeSmile.ActionFilters;
using FreeSmile.Controllers;
using FreeSmile.DTOs;
using FreeSmile.Models;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Globalization;
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

        public async Task<ReviewDto> GetReviewAsync(int user_id)
        {
            var review = await _context.Reviews.FirstOrDefaultAsync(x => x.ReviewerId == user_id);
            if (review is null)
                throw new NotFoundException(_localizer["NotFound", _localizer["yourreview"]]);

            return new ReviewDto()
            {
                Opinion = review.Opinion,
                Rating = review.Rating
            };
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

        public async Task<GetMessageDto> SendMessageAsync(SendMessageDto message, int sender_id)
        {
            User? user2 = await _context.Users.FindAsync(message.Receiver_Id);

            if (user2 is null)
                throw new GeneralException(_localizer["UserNotFound"]);

            if (await CanUsersCommunicateAsync(sender_id, (int)message.Receiver_Id!) == false)
                throw new GeneralException(_localizer["personnotavailable"]);

            Message message1 = new Message()
            {
                SenderId = sender_id,
                ReceiverId = (int)message.Receiver_Id!,
                Body = message.Message,
                Seen = sender_id == (int)message.Receiver_Id! //If sending to himself then already seen
            };

            await _context.Messages.AddAsync(message1);
            await _context.SaveChangesAsync();

            return new GetMessageDto()
            {
                MessageId = message1.MessageId,
                IsSender = true,
                SenderId = message1.SenderId,
                ReceiverId = message1.ReceiverId,
                Message = message1.Body,
                Seen = message1.Seen,
                SentAt = message1.SentAt.Humanize(culture: CultureInfo.CurrentCulture)
            };
        }

        public async Task<List<GetMessageDto>> GetChatHistoryAsync(int user_id, int other_user_id, int page, int size)
        {
            User? user2 = await _context.Users.FindAsync(other_user_id);

            if (user2 is null)
                throw new GeneralException(_localizer["UserNotFound"]);

            var messages = _context.Messages.Where(x => (x.SenderId == user_id && x.ReceiverId == other_user_id)
                                                     || (x.ReceiverId == user_id && x.SenderId == other_user_id))
                                            .OrderByDescending(x => x.MessageId)
                                            .Select(x => new GetMessageDto()
                                            {
                                                MessageId = x.MessageId,
                                                SenderId = x.SenderId,
                                                ReceiverId = x.ReceiverId,
                                                Seen = x.Seen,
                                                Message = x.Body,
                                                IsSender = x.SenderId == user_id,
                                                SentAt = x.SentAt.Humanize(default, default, CultureInfo.CurrentCulture)
                                            }).Skip(size * --page).Take(size).ToList();

            // Mark returned messages as seen if the receiver is the one requested them
            try
            {
                foreach (var item in messages.Where(x => x.ReceiverId == user_id))
                {
                    var message = await _context.Messages.FirstOrDefaultAsync(x => x.MessageId == item.MessageId);
                    message!.Seen = true;
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("{Message}", ex.Message);
            }
            return messages;
        }

        public async Task<List<RecentMessagesDto>> GetRecentMessagesAsync(int user_id, int page, int size)
        {
            throw new NotImplementedException();
        }

        public async Task<byte[]?> GetProfilePictureAsync(int auth_user_id, int other_user_id, byte size)
        {
            if (size < 1 || size > 3)
                size = 1;

            User? user1 = null, user2 = null;

            if (auth_user_id == 0 && other_user_id == 0)
                throw new UnauthorizedException(_localizer["unauthorized"]);

            user1 = await _context.Users.FindAsync(auth_user_id);
            user2 = await _context.Users.FindAsync(other_user_id);


            if (user1?.Suspended == true)
                throw new GeneralException(_localizer["UserSuspended"]);

            if (other_user_id == 0 && auth_user_id != 0) //0 1
            {
                // authenticated user asking for his profile picture
                var imagePath = MyConstants.GetProfilePicturesPath(auth_user_id, size);
                if (File.Exists(imagePath))
                    return await File.ReadAllBytesAsync(imagePath);
            }
            else if (other_user_id != 0 && auth_user_id != 0) // 1 1 
            {
                // authenticated user asking for other guy's profile picture
                if (user2 is not null)
                {
                    if (await CanUsersCommunicateAsync(auth_user_id, other_user_id) == false)
                        throw new GeneralException(_localizer["personnotavailable"]);

                    var imagePath = MyConstants.GetProfilePicturesPath(other_user_id, size);
                    if (File.Exists(imagePath))
                        return await File.ReadAllBytesAsync(imagePath);
                }
            }
            else if (other_user_id != 0 && auth_user_id == 0) // 1 0
            {
                // stranger asking for some user's profile
                var imagePath = MyConstants.GetProfilePicturesPath(other_user_id, size);
                if (File.Exists(imagePath))
                    return await File.ReadAllBytesAsync(imagePath);
            }
            return null;
        }

        public async Task<byte[]> AddUpdateProfilePictureAsync(ProfilePictureDto value, int user_id)
        {
            var userDir = MyConstants.GetProfilePicturesUser(user_id);
            if (!Directory.Exists(userDir))
            {
                Directory.CreateDirectory(userDir);
            }
            
            try
            {
            byte[] originalImage;

            using (var memoryStream = new MemoryStream())
            {
                await value.ProfilePicture.CopyToAsync(memoryStream);
                originalImage = memoryStream.ToArray();
            }

            var Ext = Path.GetExtension(value.ProfilePicture.FileName).ToLower();

            int QualityPercent = (int)(10240000.0d / value.ProfilePicture.Length);
            
            string[] paths = { string.Empty,
                               MyConstants.GetProfilePicturesPath(user_id, 1),
                               MyConstants.GetProfilePicturesPath(user_id, 2),
                               MyConstants.GetProfilePicturesPath(user_id, 3)
                                 };

            using (var image = Image.Load(originalImage))
            {
                var encoder = ExtensionToEncoder(Ext);
                int[] sizes = new int[] { 100, 250 };
                for (int i = 0; i < sizes.Length; i++)
            {
                    using (var size = image.Clone(x => x.Resize(sizes[i], 0)))
                    {
                        await size.SaveAsync(paths[i + 1], encoder);
            }
                }

                if (QualityPercent < 100)
                    image.Mutate(x => x.Resize(image.Width * QualityPercent / 100, 0));
                await image.SaveAsync(paths[3], encoder);
            }
            return await File.ReadAllBytesAsync(paths[1]);
        }
            catch (Exception ex) when (ex is UnknownImageFormatException
                                    || ex is InvalidImageContentException
                                    || ex is NotSupportedException)
            {
                if (Directory.Exists(userDir))
                    Directory.Delete(userDir, true);

                throw new GeneralException(_localizer["ImagesOnly", _localizer["SelectedPic"]]);
            }
            catch (Exception ex)
            {
                if (Directory.Exists(userDir))
                    Directory.Delete(userDir, true);
                throw;
            }
        }

        public RegularResponse DeleteProfilePictureAsync(int user_id)
        {
            var userDir = MyConstants.GetProfilePicturesUser(user_id);

            if (Directory.Exists(userDir))
                {
                Directory.Delete(userDir, true);
            }
            return RegularResponse.Success(message: _localizer["ProfilePicDeleted"]);
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

        public async Task<CommonSettingsDto> GetCommonSettingsAsync(int user_id)
        {
            throw new NotImplementedException();
        }

        public async Task<CommonSettingsDto> UpdateCommonSettingsAsync(CommonSettingsDto settings, int user_id)
        {
            throw new NotImplementedException();
        }
    }
}
