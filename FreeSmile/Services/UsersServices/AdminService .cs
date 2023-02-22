using FreeSmile.Controllers;
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
                _logger.LogError(ex.Message);
                transaction.Rollback();
                response = new()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Error = _localizer["UnknownError"],
                    NextPage = Pages.same.ToString()
                };
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
    }
}
