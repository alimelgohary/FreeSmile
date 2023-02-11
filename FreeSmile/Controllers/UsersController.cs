using FreeSmile.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using FreeSmile.DTOs;
using FreeSmile.Services;

namespace FreeSmile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IStringLocalizer<UsersController> _localizer;
        private readonly IUserService _userService;
        private readonly IPatientService _patientService;

        public UsersController(IStringLocalizer<UsersController> localizer, IUserService userService, IPatientService patientService)
        {
            _localizer = localizer;
            _userService = userService;
            _patientService = patientService;
        }
        [HttpPost("RegisterPatient")]
        public async Task<IActionResult> Register(UserRegisterDto value)
        {
            try
            {
                await _patientService.AddUserAsync(value);
                // TODO : Return a token, cookie
                return Ok(_localizer["RegistrationSuccess"]);
            }
            catch (Exception)
            {
                return BadRequest(_localizer["patientAddError"]);
            }

        }
        [HttpPost("RegisteDentist")]
        public IActionResult Register(DentistRegisterDto value)
        {

            return Ok(value);
        }

    }
}
