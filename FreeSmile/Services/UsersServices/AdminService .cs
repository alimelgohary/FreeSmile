﻿using FreeSmile.Controllers;
using FreeSmile.DTOs;
using FreeSmile.Models;
using Microsoft.Extensions.Localization;
using System.Transactions;
using static FreeSmile.Services.AuthHelper;
using static FreeSmile.Services.Helper;

namespace FreeSmile.Services
{
    public class AdminService : IAdminService
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IStringLocalizer<UsersController> _localizer;
        private readonly FreeSmileContext _context;
        private readonly IUserService _userService;

        public AdminService(ILogger<UsersController> logger, FreeSmileContext context, IStringLocalizer<UsersController> localizer, IUserService userService)
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

                var admin = new Admin()
                {
                    AdminId = response.Id
                };
                await _context.AddAsync(admin);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                
                response = new()
                {
                    Id = admin.AdminId,
                    StatusCode = StatusCodes.Status200OK,
                    Message = _localizer["RegisterSuccess"],
                    NextPage = Pages.same.ToString() // only superadmins can register admins so they will not verify them too && also no token is sent
                };
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

        public async Task<RegularResponse> ChangePassword(ChangeUnknownPasswordDto value)
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

        public async Task<bool> IsNotSuspended(int id)
        {
            return await _userService.IsNotSuspended(id);
        }

        public async Task<bool> IsNotSuspended(string usernameOrEmail)
        {
            return await _userService.IsNotSuspended(usernameOrEmail);
        }

        public async Task<bool> IsVerifiedEmail(int id)
        {
            return await _userService.IsVerifiedEmail(id);
        }

        public async Task<bool> IsVerifiedEmail(string usernameOrEmail)
        {
            return await _userService.IsVerifiedEmail(usernameOrEmail);
        }

        public async Task<bool> InitialChecks(string usernameOrEmail)
        {
            return await _userService.InitialChecks(usernameOrEmail);
        }

        public async Task<bool> InitialChecks(int id)
        {
            return await _userService.InitialChecks(id);
        }
    }
}
