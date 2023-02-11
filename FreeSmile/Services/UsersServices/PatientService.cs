using FreeSmile.Controllers;
using FreeSmile.DTOs;
using FreeSmile.Models;
using Microsoft.Extensions.Localization;
using System.Transactions;

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

        public async Task<int> AddUserAsync(UserRegisterDto userDto)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                int id = await _userService.AddUserAsync(userDto);

                var patient = new Patient()
                {
                    PatientId = id
                };
                await _context.AddAsync(patient);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return id;
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
