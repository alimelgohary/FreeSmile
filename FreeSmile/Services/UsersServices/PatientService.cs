using FreeSmile.Controllers;
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

        
    }
}
