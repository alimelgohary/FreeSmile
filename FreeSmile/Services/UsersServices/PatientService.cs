﻿using FreeSmile.ActionFilters;
using FreeSmile.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

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


    }
}
