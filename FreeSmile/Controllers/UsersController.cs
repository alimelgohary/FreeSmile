using FreeSmile.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using FreeSmile.DTOs;

namespace FreeSmile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IStringLocalizer<UsersController> _localizer;
        private readonly FreeSmileContext _context;
        public UsersController(ILogger<UsersController> logger, FreeSmileContext context, IStringLocalizer<UsersController> localizer)
        {
            _logger = logger;
            _context = context;
            _localizer = localizer;
        }
        [HttpPost("RegisterPatient")]
        public IActionResult Register(UserRegisterDto value)
        {

            return Ok(value);
        }
        [HttpPost("RegisteDentist")]
        public IActionResult Register(DentistRegisterDto value)
        {

            return Ok(value);
        }

    }
}
