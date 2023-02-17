using FreeSmile.Controllers;
using FreeSmile.DTOs;
using FreeSmile.Models;
using Microsoft.Extensions.Localization;
using System.Transactions;
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

        public async Task<ResponseDTO> AddUserAsync(UserRegisterDto userDto)
        {
            ResponseDTO responseDto;
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                responseDto = await _userService.AddUserAsync(userDto);

                var patient = new Patient()
                {
                    PatientId = responseDto.Id
                };
                await _context.AddAsync(patient);
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
    }
}
