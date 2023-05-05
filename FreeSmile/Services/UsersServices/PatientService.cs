using FreeSmile.ActionFilters;
using FreeSmile.DTOs.Posts;
using FreeSmile.DTOs.Settings;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace FreeSmile.Services
{
    public class PatientService : IPatientService
    {
        private readonly ILogger<PatientService> _logger;
        private readonly IStringLocalizer<PatientService> _localizer;
        private readonly FreeSmileContext _context;
        private readonly IUserService _userService;
        private readonly ICommonService _commonService;

        public PatientService(ILogger<PatientService> logger, FreeSmileContext context, IStringLocalizer<PatientService> localizer, IUserService userService, ICommonService commonService)
        {
            _logger = logger;
            _context = context;
            _localizer = localizer;
            _userService = userService;
            _commonService = commonService;
        }
        public async Task<GetPatientSettingsDto> GetSettingsAsync(int user_id)
        {
            GetCommonSettingsDto dto = await _commonService.GetCommonSettingsAsync(user_id);
            var patient = new GetPatientSettingsDto()
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
            };

            return patient;
        }

        public async Task<GetPatientSettingsDto> GetPublicSettingsAsync(int auth_user_id, int other_user_id)
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

        public async Task<GetPatientSettingsDto> UpdateSettingsAsync(SetPatientSettingsDto settings, int user_id)
        {
            var patient = await _commonService.UpdateCommonSettingsAsync(settings, user_id);
            return new GetPatientSettingsDto()
            {
                Phone = patient.Phone,
                Email = patient.Email,
                Birthdate = patient.Birthdate,
                VisibleMail = patient.VisibleMail,
                VisibleContact = patient.VisibleContact,
                VisibleBd = patient.VisibleBd
            };
        }

        public async Task<List<GetCaseDto>> GetDentistsCases(int user_id, int page, int size, int gov_id, int case_type_id)
        {
            bool isEnglish = Thread.CurrentThread.CurrentCulture.Name == "en";
            bool allGov = gov_id == 0;
            bool allTypes = case_type_id == 0;

            var excluded_user_ids = await _commonService.GetUserEnemiesAsync(user_id);

            var result = from cas in _context.Cases.AsNoTracking()
                         join post in _context.Posts.AsNoTracking() on cas.CaseId equals post.PostId
                         join user in _context.Users.AsNoTracking() on post.WriterId equals user.Id
                         join dentist in _context.Dentists.AsNoTracking() on user.Id equals dentist.DentistId
                         where !excluded_user_ids.Contains(post.WriterId)
                         where allGov || cas.GovernateId == gov_id
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
                             DentistInfo = new()
                             {
                                 AcademicDegree = isEnglish ? dentist.CurrentDegreeNavigation.NameEn : dentist.CurrentDegreeNavigation.NameAr,
                                 University = isEnglish ? dentist.CurrentUniversityNavigation.NameEn : dentist.CurrentUniversityNavigation.NameAr
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
