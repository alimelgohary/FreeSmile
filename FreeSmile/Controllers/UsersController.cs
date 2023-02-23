using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using FreeSmile.DTOs;
using FreeSmile.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static FreeSmile.Services.Helper;

namespace FreeSmile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IStringLocalizer<UsersController> _localizer;
        private readonly IPatientService _patientService;
        private readonly IDentistService _dentistService;
        private readonly IAdminService _adminService;
        private readonly IUserService _userService;

        public UsersController(IStringLocalizer<UsersController> localizer,
            IPatientService patientService, IDentistService dentistService, IAdminService adminService, IUserService userService)
        {
            _localizer = localizer;
            _patientService = patientService;
            _dentistService = dentistService;
            _adminService = adminService;
            _userService = userService;
        }


        [HttpPost("RegisterPatient")]
        public async Task<IActionResult> RegisterPatientAsync([FromBody] UserRegisterDto value)
        {
            var res = await _patientService.AddUserAsync(value, Response.Cookies);
            return StatusCode(res.StatusCode, res);
        }

        
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("RegisterAdmin")]
        public async Task<IActionResult> RegisterAdminAsync([FromBody] UserRegisterDto value)
        {
            var res = await _adminService.AddUserAsync(value, Response.Cookies);
            return StatusCode(res.StatusCode, res);
        }


        [HttpPost("RegisterDentist")]
        public async Task<IActionResult> RegisterDentistAsync([FromBody] UserRegisterDto value)
        {
            var res = await _dentistService.AddUserAsync(value, Response.Cookies);
            return StatusCode(res.StatusCode, res);
        }


        [HttpPut("VerifyAccount")]
        [Authorize]
        public async Task<IActionResult> VerifyMyAccount([FromBody] OtpDto dto)
        {
            string? user_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(user_id))
                return Unauthorized();

            int user_id_int = int.Parse(user_id);
            RegularResponse res = await _userService.VerifyAccount(dto.Otp, user_id_int);
            return StatusCode(res.StatusCode, res);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto value)
        {
            var res = await _userService.Login(value, Response.Cookies);
            return StatusCode(res.StatusCode, res);
        }

        [HttpPut("RequestOtp")]
        [Authorize]
        public async Task<IActionResult> RequestEmailOtp()
        {
            string? user_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(user_id))
                return Unauthorized();

            int user_id_int = int.Parse(user_id);
            RegularResponse res = await _userService.RequestEmailOtp(user_id_int);
            return StatusCode(res.StatusCode, res);
        }

        [HttpPut("ForgotPassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangeUnknownPasswordDto dto)
        {
            var res = await _userService.ChangePassword(dto);
            return StatusCode(res.StatusCode, res);
        }
        
        [Authorize]
        [HttpPut("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangeKnownPasswordDto dto)
        {
            string? user_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(user_id))
                return Unauthorized();

            int user_id_int = int.Parse(user_id);
            RegularResponse res = await _userService.ChangePassword(dto, user_id_int);
            return StatusCode(res.StatusCode, res);
        }
    }
}

