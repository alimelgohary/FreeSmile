using FreeSmile.Controllers;
using FreeSmile.DTOs;
using FreeSmile.Models;
using Microsoft.Extensions.Localization;
using System.Transactions;
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

        public async Task<ResponseDTO> AddUserAsync(UserRegisterDto userDto)
        {
            ResponseDTO responseDto;
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                responseDto = await _userService.AddUserAsync(userDto);

                var admin = new Admin()
                {
                    AdminId = responseDto.Id
                };
                await _context.AddAsync(admin);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return responseDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                transaction.Rollback();
                throw;
            }

        }

        public Task<ResponseDTO> VerifyAccount(string otp, int user_id)
        {
            return _userService.VerifyAccount(otp, user_id);
        }
    }
}
