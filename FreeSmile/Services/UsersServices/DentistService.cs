using FreeSmile.ActionFilters;
using FreeSmile.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using static FreeSmile.Services.Helper;
using static FreeSmile.Services.DirectoryHelper;
using FreeSmile.DTOs.Settings;
using FreeSmile.DTOs.Auth;
using FreeSmile.DTOs.Posts;
using System.Globalization;
using Humanizer;

namespace FreeSmile.Services
{
    public class DentistService : IDentistService
    {
        private readonly ILogger<DentistService> _logger;
        private readonly IStringLocalizer<DentistService> _localizer;
        private readonly FreeSmileContext _context;
        private readonly IUserService _userService;
        private readonly ICommonService _commonService;

        public DentistService(ILogger<DentistService> logger, FreeSmileContext context, IStringLocalizer<DentistService> localizer, IUserService userService, ICommonService commonService)
        {
            _logger = logger;
            _context = context;
            _localizer = localizer;
            _userService = userService;
            _commonService = commonService;
        }

        public async Task<RegularResponse> AddVerificationRequestAsync(VerificationDto verificationDto, int ownerId)
        {
            if (await _context.VerificationRequests.AnyAsync(v => v.OwnerId == ownerId))
                throw new GeneralException(_localizer["AlreadyRequested"]);

            var userDir = GetVerificationPathUser(ownerId);
            if (!Directory.Exists(userDir))
            {
                Directory.CreateDirectory(userDir);
            }

            var natPath = GetVerificationImgPath(ownerId, VerificationType.Nat);
            var proofPath = GetVerificationImgPath(ownerId, VerificationType.Proof);

            try
            {
                byte[] natImage;
                using (var memoryStream = new MemoryStream())
                {
                    await verificationDto.NatIdPhoto.CopyToAsync(memoryStream);
                    natImage = memoryStream.ToArray();
                }
                var natExt = Path.GetExtension(verificationDto.NatIdPhoto.FileName);
                var encoder = ExtensionToEncoder(natExt);
                using (var image = Image.Load(natImage))
                {
                    await image.SaveAsync(natPath, encoder);
                }

                byte[] proofImage;
                using (var memoryStream = new MemoryStream())
                {
                    await verificationDto.ProofOfDegreePhoto.CopyToAsync(memoryStream);
                    proofImage = memoryStream.ToArray();
                }
                var proofExt = Path.GetExtension(verificationDto.ProofOfDegreePhoto.FileName);
                encoder = ExtensionToEncoder(proofExt);
                using (var image = Image.Load(proofImage))
                {
                    await image.SaveAsync(proofPath, encoder);
                }

                await _context.AddAsync(
                    new VerificationRequest()
                    {
                        OwnerId = ownerId,
                        DegreeRequested = (int)verificationDto.DegreeRequested!,
                        UniversityRequested = (int)verificationDto.UniversityRequested!
                    });

                await _context.SaveChangesAsync();

                return RegularResponse.Success(message: _localizer["VerificationRequestSuccess"], nextPage: Pages.pendingVerificationAcceptance.ToString());

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

        public async Task<RegularResponse> DeleteVerificationRequestAsync(int user_id_int)
        {
            var CurrentRequest = await _context.VerificationRequests.FirstOrDefaultAsync(x => x.OwnerId == user_id_int);
            if (CurrentRequest is null)
                throw new NotFoundException(_localizer["NotFound", _localizer["request"]]);

            _context.Remove(CurrentRequest);
            await _context.SaveChangesAsync();

            var userDir = GetVerificationPathUser(user_id_int);
            if (Directory.Exists(userDir))
                Directory.Delete(userDir, true);

            return RegularResponse.Success(message: _localizer["DeletedRequest"],
                                            nextPage: Pages.pendingVerificationAcceptance.ToString());
        }
        public async Task<GetDentistSettingsDto> GetSettingsAsync(int user_id)
        {
            var lang = Thread.CurrentThread.CurrentCulture.Name;
            GetCommonSettingsDto dto = await _commonService.GetCommonSettingsAsync(user_id);
            var dentist = await _context.Dentists.AsNoTracking()
                                            .Select(x => new GetDentistSettingsDto()
                                            {
                                                UserId = dto.UserId,
                                                FullName = dto.FullName,
                                                Username = dto.Username,
                                                Phone = dto.Phone,
                                                Email = dto.Email,
                                                Birthdate = dto.Birthdate,
                                                VisibleMail = dto.VisibleMail,
                                                VisibleContact = dto.VisibleContact,
                                                ProfilePicture = dto.ProfilePicture,
                                                AcademicDegree = x.CurrentDegreeNavigation.Lang(lang),
                                                University = x.CurrentUniversityNavigation.Lang(lang),
                                                Bio = x.Bio,
                                                FbUsername = x.FbUsername,
                                                LinkedUsername = x.LinkedUsername,
                                                GScholarUsername = x.GScholarUsername,
                                                ResearchGateUsername = x.ResearchGateUsername,
                                            })
                                            .FirstOrDefaultAsync(x => x.UserId == user_id);
            dentist!.HasPendingVerificationRequest = await _context.VerificationRequests.AnyAsync(x => x.OwnerId == user_id);
            return dentist;
        }

        public async Task<GetDentistSettingsDto> GetPublicSettingsAsync(int auth_user_id, int other_user_id)
        {
            if (auth_user_id != 0)
            {
                // Authenticated user asking for other user settings
                var user1 = await _context.Users.AsNoTracking().Select(x => new { x.Id, x.Suspended }).FirstOrDefaultAsync(x => x.Id == auth_user_id);
                if (user1?.Suspended == true)
                    throw new GeneralException(_localizer["UserSuspended"]);

                if (await _commonService.CanUsersCommunicateAsync(auth_user_id, other_user_id) == false)
                    throw new GeneralException(_localizer["personnotavailable"]);
            }

            var settings = await GetSettingsAsync(other_user_id);
            settings.HidePrivate();

            return settings;
        }

        public async Task<GetDentistSettingsDto> UpdateSettingsAsync(SetDentistSettingsDto settings, int user_id)
        {
            await _commonService.UpdateCommonSettingsAsync(settings, user_id);
            Dentist? dentist = await _context.Dentists.FindAsync(user_id);

            dentist!.Bio = settings.Bio;
            dentist.FbUsername = settings.FbUsername;
            dentist.LinkedUsername = settings.LinkedUsername;
            dentist.GScholarUsername = settings.GScholarUsername;
            dentist.ResearchGateUsername = settings.ResearchGateUsername;
            await _context.SaveChangesAsync();

            return await GetSettingsAsync(user_id);
        }

        public async Task<List<GetCaseDto>> GetPatientsCasesAsync(int user_id, int page, int size, int gov_id, int case_type_id)
        {
            bool isEnglish = Thread.CurrentThread.CurrentCulture.Name == "en";
            bool allTypes = case_type_id == 0;
            if (gov_id == 0)
            {
                var dentist = await _context.Dentists.AsNoTracking()
                                                     .Select(x =>
                                                        new
                                                        {
                                                            x.DentistId,
                                                            gov_id = x.CurrentUniversityNavigation.Gov.GovId
                                                        }).FirstOrDefaultAsync(x => x.DentistId == user_id);
                gov_id = dentist!.gov_id;
            }

            var excluded_user_ids = await _commonService.GetUserEnemiesAsync(user_id);

            var result = from cas in _context.Cases.AsNoTracking()
                         join post in _context.Posts.AsNoTracking() on cas.CaseId equals post.PostId
                         join user in _context.Users.AsNoTracking() on post.WriterId equals user.Id
                         join patient in _context.Patients.AsNoTracking() on user.Id equals patient.PatientId
                         where !excluded_user_ids.Contains(post.WriterId)
                         where cas.GovernateId == gov_id
                         where allTypes || cas.CaseTypeId == case_type_id
                         orderby post.TimeUpdated ?? post.TimeWritten descending
                         select new GetCaseDto
                         {
                             UserInfo = new()
                             {
                                 UserId = post.WriterId,
                                 FullName = user.Fullname,
                                 Username = user.Username,
                                 ProfilePicture = null,
                             },
                             PostId = post.PostId,
                             Title = post.Title,
                             Body = post.Body,
                             TimeWritten = post.TimeWritten.Humanize(default, default, CultureInfo.CurrentCulture),
                             TimeUpdated = post.TimeUpdated != null ? post.TimeUpdated.Humanize(default, default, CultureInfo.CurrentCulture) : null,
                             Images = null,
                             Phone = user.VisibleContact ? user.Phone : null,
                             Governorate = isEnglish ? cas.Governate.NameEn : cas.Governate.NameAr,
                             CaseType = isEnglish ? cas.CaseType.NameEn : cas.CaseType.NameAr,
                         };

            var list = result.Skip(--page * size).Take(size).ToList();
            await Parallel.ForEachAsync(list, async (post, cancellationToken) =>
            {
                post.UserInfo.ProfilePicture = await _commonService.GetProfilePictureDangerousAsync(post.UserInfo.UserId, 1);
                post.Images = await _commonService.GetPostImagesAsync(post.PostId);
            });

            return list;
        }
    }
}

