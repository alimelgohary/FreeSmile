﻿using FreeSmile.Controllers;
using FreeSmile.DTOs;
using FreeSmile.Models;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using System.Transactions;
using static FreeSmile.Services.AuthHelper;
using static FreeSmile.Services.Helper;

namespace FreeSmile.Services
{
    public class PatientService : IPatientService
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IStringLocalizer<UsersController> _localizer;
        private readonly FreeSmileContext _context;
        private readonly IUserService _userService;

        public PatientService(ILogger<UsersController> logger, FreeSmileContext context, IStringLocalizer<UsersController> localizer, IUserService userService)
        {
            _logger = logger;
            _context = context;
            _localizer = localizer;
            _userService = userService;
        }

        public async Task<RegularResponse> AddUserAsync(UserRegisterDto userDto, IResponseCookies cookies)
        {
            RegularResponse response;
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                response = await _userService.AddUserAsync(userDto, cookies);

                var patient = new Patient()
                {
                    PatientId = response.Id
                };
                await _context.AddAsync(patient);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var tokenAge = MyConstants.REGISTER_TOKEN_AGE;
                string token = GetToken(patient.PatientId, tokenAge, Role.Patient);
                cookies.Append(MyConstants.AUTH_COOKIE_KEY, token, new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.None, MaxAge = tokenAge, Secure = true });

                response = RegularResponse.Success(
                    id : patient.PatientId,
                    token : token,
                    message: _localizer["RegisterSuccess"],
                    nextPage: Pages.verifyEmail.ToString()
                );
            }
            catch (Exception ex)
            {
                _logger.LogError("{Message}", ex.Message);
                transaction.Rollback();
                response = RegularResponse.UnknownError(_localizer);
            }
            return response;
        }

        public Task<RegularResponse> VerifyAccount(string otp, int user_id)
        {
            return _userService.VerifyAccount(otp, user_id);
        }

        public async Task<RegularResponse> Login(UserLoginDto value, IResponseCookies cookies)
        {
            return await _userService.Login(value, cookies);
        }

        public async Task<RegularResponse> RequestEmailOtp(int user_id)
        {
            return await _userService.RequestEmailOtp(user_id);
        }

        public async Task<RegularResponse> ChangePassword(ResetPasswordDto value)
        {
            return await _userService.ChangePassword(value);
        }

        public async Task<RegularResponse> ChangePassword(ChangeKnownPasswordDto value, int user_id_int)
        {
            return await _userService.ChangePassword(value, user_id_int);
        }
        public async Task<RegularResponse> RequestEmailOtp(string usernameOrEmail)
        {
            return await _userService.RequestEmailOtp(usernameOrEmail);
        }

        public async Task<RegularResponse> RedirectToHome(int user_id)
        {
            return await _userService.RedirectToHome(user_id);
        }
    }
}
