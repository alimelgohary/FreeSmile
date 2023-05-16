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

            var result = from cas in _context.PatientHomeViews.AsNoTracking()
                         where !excluded_user_ids.Contains(cas.UserId)
                         where allGov || cas.GovId == gov_id
                         where allTypes || cas.CaseTypeId == case_type_id
                         orderby cas.TimeUpdated ?? cas.TimeWritten descending
                         select new GetCaseDto
                         {
                             UserInfo = new()
                             {
                                 UserId = cas.UserId,
                                 FullName = cas.FullName,
                                 Username = cas.Username,
                                 ProfilePicture = null,
                             },
                             DentistInfo = new()
                             {
                                 AcademicDegree = isEnglish ? cas.AcademicDegreeEn : cas.AcademicDegreeAr,
                                 University = isEnglish ? cas.CurrentUnivrsityEn : cas.CurrentUnivrsityAr,
                             },
                             PostId = cas.PostId,
                             Title = cas.Title,
                             Body = cas.Body,
                             TimeWritten = cas.TimeWritten.Humanize(default, default, CultureInfo.CurrentCulture),
                             TimeUpdated = cas.TimeUpdated == null ? null : cas.TimeUpdated.Humanize(default, default, CultureInfo.CurrentCulture),
                             Written = (DateTime)cas.TimeWritten!,
                             Updated = cas.TimeUpdated,
                             Images = null,
                             Phone = cas.Phone,
                             Governorate = isEnglish ? cas.GovNameEn : cas.GovNameAr,
                             CaseType = isEnglish ? cas.CaseTypeEn : cas.CaseTypeAr,
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
