using FreeSmile.ActionFilters;
using FreeSmile.Controllers;
using FreeSmile.DTOs;
using FreeSmile.Models;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Globalization;
using static FreeSmile.Services.Helper;
using static FreeSmile.Services.DirectoryHelper;
using static FreeSmile.Services.MyConstants;
using static FreeSmile.Services.AuthHelper;

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

        public async Task<Role> GetCurrentRole(int user_id)
        {
            if (await _context.Patients.AnyAsync(x => x.PatientId == user_id))
                return Role.Patient;

            if (await _context.Dentists.AnyAsync(x => x.DentistId == user_id))
                return Role.Dentist;

            if (await _context.Admins.AnyAsync(x => x.AdminId == user_id))
                return Role.Admin;

            if (await _context.SuperAdmins.AnyAsync(x => x.SuperAdminId == user_id))
                return Role.SuperAdmin;

            return Role.Patient;
        }

        public async Task DeletePostDangerousAsync(int id)
        {
            await _context.Database.ExecuteSqlRawAsync($"dbo.DeletePost {id};");

            var postsImgsDir = GetPostsPathPost(id);
            if (Directory.Exists(postsImgsDir))
                Directory.Delete(postsImgsDir, true);
        }

        public async Task<bool> CanUsersCommunicateAsync(int user_id, int other_user_id)
        {
            var user2 = await _context.Users.AsNoTracking()
                                            .Select(x => new
                                            {
                                                x.Id,
                                                x.Suspended,
                                                Blockeds = x.Blockeds.Select(x => x.Id),
                                                Blockers = x.Blockers.Select(x => x.Id)
                                            })
                                            .FirstOrDefaultAsync(x => x.Id == other_user_id);
            if (user2?.Suspended == true)
                return false;

            bool user2_blocked_user1 = user2?.Blockeds.Where(x => x == user_id).Count() > 0;
            bool user2_is_blocked_by_user1 = user2?.Blockers.Where(x => x == user_id).Count() > 0;

            return !user2_blocked_user1 && !user2_is_blocked_by_user1;
        }

        public async Task<ReviewDto> GetReviewAsync(int user_id)
        {
            var review = await _context.Reviews
                                .AsNoTracking()
                                .FirstOrDefaultAsync(x => x.ReviewerId == user_id);
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

            notifications = await _context.Notifications
                                .AsNoTracking()
                                .Where(x => x.OwnerId == user_id)
                                .OrderByDescending(x => x.NotificationId)
                                .Skip(size * --page)
                                .Take(size)
                                .Include(x => x.Temp)
                                .ToListAsync();
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

        public async Task<List<GetBlockedUsersDto>> GetBlockedListAsync(int user_id, int page, int size)
        {
            User? user = await _context.Users.Include(x => x.Blockeds)
                                             .FirstOrDefaultAsync(x => x.Id == user_id);

            return user!.Blockeds.Select(x => new GetBlockedUsersDto()
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
                var imagePath = GetProfilePicturesPath(auth_user_id, size);
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

                    var imagePath = GetProfilePicturesPath(other_user_id, size);
                    if (File.Exists(imagePath))
                        return await File.ReadAllBytesAsync(imagePath);
                }
            }
            else if (other_user_id != 0 && auth_user_id == 0) // 1 0
            {
                // stranger asking for some user's profile
                var imagePath = GetProfilePicturesPath(other_user_id, size);
                if (File.Exists(imagePath))
                    return await File.ReadAllBytesAsync(imagePath);
            }
            return null;
        }

        public async Task<byte[]> AddUpdateProfilePictureAsync(AddProfilePictureDto value, int user_id)
        {
            var userDir = GetProfilePicturesUser(user_id);
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

            string[] paths = { string.Empty,
                               GetProfilePicturesPath(user_id, 1),
                               GetProfilePicturesPath(user_id, 2),
                               GetProfilePicturesPath(user_id, 3)
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

                    while (value.ProfilePicture.Length >= MAX_IMAGE_SIZE)
                    {
                        image.Mutate(x => x.Resize(image.Width * 70 / 100, 0));
                        using (var ms = new MemoryStream())
                        {
                            await image.SaveAsync(ms, encoder);
                            if (ms.Length <= MAX_IMAGE_SIZE)
                                break;
                        }
                    }
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
            catch (Exception)
            {
                if (Directory.Exists(userDir))
                    Directory.Delete(userDir, true);
                throw;
            }
        }

        public RegularResponse DeleteProfilePictureAsync(int user_id)
        {
            var userDir = GetProfilePicturesUser(user_id);

            if (Directory.Exists(userDir))
                {
                Directory.Delete(userDir, true);
            }
            return RegularResponse.Success(message: _localizer["ProfilePicDeleted"]);
        }

        public async Task<int> AddCaseAsync(AddCaseDto value, int user_id)
        {
            Role role = await GetCurrentRole(user_id);
            if (value.GovernorateId == 0 && role == Role.Patient)
                throw new GeneralException(_localizer["Required", _localizer["GovernorateId"]]);

            int gov_id;
            if (role == Role.Patient)
            {
                gov_id = value.GovernorateId!;
            }
            else
            {
                Dentist? d = await _context.Dentists.Include(x => x.CurrentUniversityNavigation).FirstOrDefaultAsync(x => x.DentistId == user_id);
                gov_id = d!.CurrentUniversityNavigation.GovId;
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var post_id = await AddPostAsync(value, user_id);

                var case1 = new Case()
                {
                    CaseId = post_id,
                    CaseTypeId = (int)value.CaseTypeId!,
                    GovernateId = value.GovernorateId
                };
                await _context.AddAsync(case1);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return case1.CaseId;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> AddPostAsync(AddPostDto value, int user_id)
        {
            Post post = new()
            {
                Title = value.Title,
                Body = value.Body,
                WriterId = user_id
            };

            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            var post_dir = GetPostsPathPost(post.PostId);
            try
            {
                if (!Directory.Exists(post_dir))
                    Directory.CreateDirectory(post_dir);

                int count = value.Images is not null ? value.Images.Count : 0;
                for (int i = 0; i < count; i++)
                {
                    await ResizeSaveImage(GetPostsPathImg(post.PostId, i), value.Images![i]);
                }
                return post.PostId;
            }
            catch (Exception ex) when (ex is UnknownImageFormatException
                                    || ex is InvalidImageContentException
                                    || ex is NotSupportedException)
            {
                if (Directory.Exists(post_dir))
                    Directory.Delete(post_dir, true);

                throw new GeneralException(_localizer["ImagesOnly", _localizer["SelectedPic"]]);
            }
            catch (Exception)
        {
                if (Directory.Exists(post_dir))
                    Directory.Delete(post_dir, true);
                throw;
            }

        }

        public async Task<RegularResponse> UpdateCaseAsync(UpdateCaseDto value, int user_id)
        {
            Case? case1 = await _context.Cases.Include(x => x.CaseNavigation)
                                              .FirstOrDefaultAsync(x=>x.CaseId == (int)value.updated_post_id! && x.CaseNavigation.WriterId == user_id);
            if (case1 is null)
                throw new NotFoundException(_localizer["notfound", _localizer["thispost"]]);

            Role role = await GetCurrentRole(user_id);
            if (value.GovernorateId == 0 && role == Role.Patient)
                throw new GeneralException(_localizer["Required", _localizer["GovernorateId"]]);

            int gov_id;
            if (role == Role.Patient)
            {
                gov_id = value.GovernorateId!;
            }
            else
            {
                Dentist? d = await _context.Dentists.Include(x => x.CurrentUniversityNavigation).FirstOrDefaultAsync(x => x.DentistId == user_id);
                gov_id = d!.CurrentUniversityNavigation.GovId;
            }

            case1.GovernateId = gov_id;
            case1.CaseTypeId = (int)value.CaseTypeId!;
            case1.CaseNavigation.Title = value.Title;
            case1.CaseNavigation.Body = value.Body;
            case1.CaseNavigation.TimeUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RegularResponse.Success(message: _localizer["PostEditedSuccess"]);
        }

        public async Task<RegularResponse> DeletePostAsync(int user_id, int case_post_id)
        {
            Role role = await GetCurrentRole(user_id);
            if (role == Role.Admin || role == Role.SuperAdmin)
            {
                await DeletePostDangerousAsync(case_post_id);
                return RegularResponse.Success(message: _localizer["PostDeleted"]);
            }

            if (await _context.Posts.FirstOrDefaultAsync(x => x.PostId == case_post_id && x.WriterId == user_id) is not null)
        {
                await DeletePostDangerousAsync(case_post_id);
                return RegularResponse.Success(message: _localizer["PostDeleted"]);
            }

            throw new NotFoundException(_localizer["notfound", _localizer["thispost"]]);
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
