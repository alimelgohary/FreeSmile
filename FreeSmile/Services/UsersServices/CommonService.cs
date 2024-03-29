﻿using FreeSmile.ActionFilters;
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
using System.Diagnostics.CodeAnalysis;
using FreeSmile.DTOs.Settings;
using FreeSmile.DTOs.Posts;

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
            if (user_id == 0 || other_user_id == 0 || user_id == other_user_id)
                return true;

            var user2 = await _context.Users.AsNoTracking()
                                            .Select(x => new
                                            {
                                                x.Id,
                                                x.Suspended,
                                                Blockeds = x.Blockeds.Select(x => x.Id),
                                                Blockers = x.Blockers.Select(x => x.Id)
                                            })
                                            .FirstOrDefaultAsync(x => x.Id == other_user_id 
                                            && (x.Blockers.Contains(user_id) || x.Blockeds.Contains(user_id) || x.Suspended == true));
            return user2 == null;
        }

        public async Task<IEnumerable<int>> GetUserEnemiesAsync(int user_id)
        {
            if (user_id <= 0)
                return Enumerable.Empty<int>();

            var excluded = await _context.Users.AsNoTracking()
                                         .Select(x =>
                                            new
                                            {
                                                x.Id,
                                                Blockeds = x.Blockeds.Select(x => x.Id),
                                                Blockers = x.Blockers.Select(x => x.Id)
                                            })
                                         .FirstOrDefaultAsync(x => x.Id == user_id);

            var suspended = _context.Users.Where(x => x.Suspended).Select(x => x.Id);

            return excluded!.Blockeds.Concat(excluded.Blockers).Concat(suspended);
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
                    Opinion = value.Opinion?.Trim(),
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
            string lang = Thread.CurrentThread.CurrentCulture.Name;
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
                    Body = string.Format(notification.Temp.Lang(lang), notification.ActorUsername, notification.PostTitle?.Truncate(20), notification.Likes, notification.Comments),
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
            bool previousReport = await _context.PostReports.AnyAsync(x => x.ReporterId == user_id && x.PostId == value.reported_post_id);
            if (previousReport)
                throw new GeneralException(_localizer["AlreadyReportedThisPost"]);

            await _context.PostReports.AddAsync(new()
            {
                ReporterId = user_id,
                PostId = (int)value.reported_post_id!,
                Reason = value.Reason?.Trim()
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
            page--;
            var blocklist = await _context.Users.AsNoTracking()
                                                .Select(x =>
                                                    new
                                                    {
                                                        x.Id,
                                                        Blockeds = x.Blockeds.Skip(page * size)
                                                                             .Take(size)
                                                                             .Select(x => new GetBlockedUsersDto
                                                                             {
                                                                                 Username = x.Username,
                                                                                 User_Id = x.Id,
                                                                                 Full_Name = x.Fullname
                                                                             })
                                                    })
                                                .FirstOrDefaultAsync(x => x.Id == user_id);

            if (blocklist is not null)
                return blocklist.Blockeds.ToList();
            return new();
        }

        public async Task<GetMessageDto> SendMessageAsync(SendMessageDto message, int sender_id)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == message.Receiver_Id))
                throw new GeneralException(_localizer["UserNotFound"]);

            if (await CanUsersCommunicateAsync(sender_id, (int)message.Receiver_Id!) == false)
                throw new GeneralException(_localizer["personnotavailable"]);

            Message message1 = new Message()
            {
                SenderId = sender_id,
                ReceiverId = (int)message.Receiver_Id!,
                Body = message.Message.Trim(),
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

        public async Task<List<GetMessageDto>> GetChatHistoryAsync(int user_id, int other_user_id, int page, int size, int after)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == other_user_id))
                throw new GeneralException(_localizer["UserNotFound"]);

            bool allMessages = after == 0;
            var messages = _context.Messages.AsNoTracking()
                                            .Where(x => (x.SenderId == user_id && x.ReceiverId == other_user_id)
                                                     || (x.ReceiverId == user_id && x.SenderId == other_user_id))
                                            .Where(x => (allMessages || x.MessageId > after))
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
                    (await _context.Messages.FindAsync(item.MessageId))!.Seen = true;
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("{Message}", ex.Message);
            }
            return messages;
        }

        public async Task<List<RecentMessagesDto>> GetRecentMessagesAsync(int user_id, int page, int size, string? q)
        {
            List<RecentMessagesDto> returnedList = new();

            bool allMessages = string.IsNullOrEmpty(q);
            var lastMessages = (from m in _context.Messages.AsNoTracking()
                                where m.SenderId == user_id || m.ReceiverId == user_id
                                where m.MessageId == (
                                    from msg in _context.Messages.AsNoTracking()
                                    where (msg.SenderId == m.SenderId && msg.ReceiverId == m.ReceiverId) ||
                                          (msg.SenderId == m.ReceiverId && msg.ReceiverId == m.SenderId)
                                    select msg.MessageId
                                ).Max()
                                join u in _context.Users.AsNoTracking() on user_id == m.SenderId ? m.ReceiverId : m.SenderId equals u.Id
                                where allMessages || (u.Username + " " + u.Fullname).Contains(q!)
                                orderby m.MessageId descending
                                select new RecentMessagesDto
                                {
                                    UserId = u.Id,
                                    FullName = u.Fullname,
                                    Username = u.Username,
                                    Seen = m.Seen,
                                    LastMessage = m.Body,
                                    LastMessageTime = m.SentAt.Humanize(default, default, CultureInfo.CurrentCulture),
                                    IsSender = user_id == m.SenderId,
                                })
                             .Skip(--page * size)
                             .Take(size)
                             .ToList();

            var userEnemies = await GetUserEnemiesAsync(user_id);
            foreach (var message in lastMessages)
            {
                message.IsAvailable = userEnemies.Contains(message.UserId) == false;
                if (message.IsAvailable)
                    message.ProfilePicture = await GetProfilePictureDangerousAsync(message.UserId);
            }

            if (!allMessages && lastMessages.Count < size) // If user is searching and Size not met then load some other users
            {
                var users = _context.Users
                                    .Where(u => !userEnemies.Contains(u.Id))
                                    .Where(u => !lastMessages.Select(x => x.UserId).Contains(u.Id))
                                    .Where(u => (u.Username + " " + u.Fullname).Contains(q!))
                                    .Take(size - lastMessages.Count)
                                    .Select(u => new RecentMessagesDto
                                    {
                                        UserId = u.Id,
                                        FullName = u.Fullname,
                                        Username = u.Username,
                                    });

                lastMessages.AddRange(users);
            }
            return lastMessages;
        }
        public class PairEqualityComparer : IEqualityComparer<Message>
        {
            public bool Equals(Message? x, Message? y)
            {
                if (x is null && y is null)
                    return true;
                if (x is null || y is null)
                    return false;
                if ((x.SenderId, x.ReceiverId) == (y.ReceiverId, y.SenderId))
                    return true;
                if ((x.SenderId, x.ReceiverId) == (y.SenderId, y.ReceiverId))
                    return true;
                return false;
            }

            public int GetHashCode([DisallowNull] Message obj)
            {
                return obj.SenderId.GetHashCode() ^ obj.ReceiverId.GetHashCode();
            }
        }
        public async Task<byte[]?> GetProfilePictureAsync(int auth_user_id, int other_user_id, byte size)
        {
            if (size < 1 || size > 3)
                size = 1;

            if (auth_user_id == 0 && other_user_id == 0)
                throw new UnauthorizedException(_localizer["unauthorized"]);

            var user2 = await _context.Users.AsNoTracking().Select(x => new { x.Id, x.Suspended }).FirstOrDefaultAsync(x => x.Id == other_user_id);

            if ((other_user_id == 0 && auth_user_id != 0) || auth_user_id == other_user_id) //0 1
            {
                // authenticated user asking for his profile picture
                return await GetProfilePictureDangerousAsync(auth_user_id, size);
            }
            else if (other_user_id != 0 && auth_user_id != 0) // 1 1 
            {
                // authenticated user asking for other guy's profile picture
                if (user2 is not null)
                {
                    if (await CanUsersCommunicateAsync(auth_user_id, other_user_id) == false)
                        throw new GeneralException(_localizer["personnotavailable"]);

                    return await GetProfilePictureDangerousAsync(other_user_id, size);
                }
            }
            else if (other_user_id != 0 && auth_user_id == 0) // 1 0
            {
                // stranger asking for some user's profile
                return await GetProfilePictureDangerousAsync(other_user_id, size);
            }
            return null;
        }

        public async Task<byte[]?> GetProfilePictureDangerousAsync(int user_id, byte size = 1)
        {
            var imagePath = GetProfilePicturesPath(user_id, size);
            if (File.Exists(imagePath))
                return await File.ReadAllBytesAsync(imagePath);
            return null;
        }
        public async Task<byte[]> AddUpdateProfilePictureAsync(AddProfilePictureDto value, int user_id)
        {
            var userDir = GetProfilePicturesUser(user_id);
            if (!Directory.Exists(userDir))
                Directory.CreateDirectory(userDir);

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

        public async Task<int> AddCaseAsync(AddCaseDto value, int user_id, string roleString)
        {
            Role role = Enum.Parse<Role>(roleString);
            if (value.GovernorateId == 0 && role == Role.Patient)
                throw new GeneralException(_localizer["Required", _localizer["GovernorateId"]]);

            int gov_id;
            if (role == Role.Patient)
            {
                gov_id = value.GovernorateId!;
            }
            else
            {
                gov_id = (await _context.Dentists.AsNoTracking()
                                                 .Select(x => new
                                                 {
                                                     x.DentistId,
                                                     x.CurrentUniversityNavigation.GovId
                                                 }).FirstOrDefaultAsync(x => x.DentistId == user_id))!.GovId;
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var post_id = await AddPostAsync(value, user_id);

                var case1 = new Case()
                {
                    CaseId = post_id,
                    CaseTypeId = (int)value.CaseTypeId!,
                    GovernateId = gov_id
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

        public async Task<List<byte[]>?> GetPostImagesAsync(int postId)
        {
            var path = GetPostsPathPost(postId);
            if (!Directory.Exists(path))
                return new();

            var paths = Directory.GetFiles(path);
            Array.Sort(paths);

            List<byte[]> imageList = new(paths.Length);
            List<Task<byte[]>> tasks = new(paths.Length);

            foreach (var item in paths)
                tasks.Add(Task.Run(async () => await File.ReadAllBytesAsync(item)));

            Task.WaitAll(tasks.ToArray());

            foreach (var task in tasks)
                if (task.Exception == null && task.Result != null)
                    imageList.Add(task.Result);

            return imageList;
        }

        public async Task<RegularResponse> UpdateCaseAsync(UpdateCaseDto value, int user_id, string roleString)
        {
            Case? case1 = await _context.Cases.Include(x => x.CaseNavigation)
                                              .FirstOrDefaultAsync(x => x.CaseId == (int)value.post_id! && x.CaseNavigation.WriterId == user_id);
            if (case1 is null)
                throw new NotFoundException(_localizer["notfound", _localizer["thispost"]]);

            Role role = Enum.Parse<Role>(roleString);
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

        public async Task<RegularResponse> DeletePostAsync(int user_id, int case_post_id, string roleString)
        {
            Role role = Enum.Parse<Role>(roleString);
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

        public async Task<GetCommonSettingsDto> GetCommonSettingsAsync(int user_id)
        {
            var user = await _context.Users.AsNoTracking()
                                            .Select(x => new GetCommonSettingsDto()
                                            {
                                                UserId = x.Id,
                                                FullName = x.Fullname,
                                                Username = x.Username,
                                                Phone = x.Phone,
                                                Email = x.Email,
                                                Birthdate = x.Bd == null ? null : ((DateTime)x.Bd).ToString("yyyy-MM-dd"),
                                                VisibleMail = x.VisibleMail,
                                                VisibleContact = x.VisibleContact,
                                                VisibleBd = x.VisibleBd
                                            })
                                            .FirstOrDefaultAsync(x => x.UserId == user_id);
            user!.ProfilePicture = await GetProfilePictureDangerousAsync(user_id, size: 1);

            return user;
        }

        public async Task<GetCommonSettingsDto> UpdateCommonSettingsAsync(SetCommonSettingsDto settings, int user_id)
        {
            User? user = await _context.Users.FindAsync(user_id);

            if (settings.Username != null)
                user!.Username = settings.Username;

            user!.Fullname = settings.Fullname.Trim();
            user.Phone = settings.Phone;
            user.Bd = settings.Birthdate is null ? null : DateTime.Parse(settings.Birthdate);
            user.VisibleMail = settings.VisibleMail;
            user.VisibleContact = settings.VisibleContact;
            user.VisibleBd = settings.VisibleBd;

            await _context.SaveChangesAsync();

            return await GetCommonSettingsAsync(user_id);
        }

        public async Task AddNotificationDangerousAsync(int owner_id, NotificationTemplates temp_name, string? post_title = null, int? post_id = null, string? actor_username = null, int? likes = null, int? comments = null)
        {
            await _context.Notifications.AddAsync(new Notification()
            {
                OwnerId = owner_id,
                TempId = (int)temp_name,
                PostTitle = post_title,
                PostId = post_id,
                ActorUsername = actor_username
            });
            await _context.SaveChangesAsync();
        }
    }
}
