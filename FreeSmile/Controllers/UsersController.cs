using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using FreeSmile.DTOs;
using FreeSmile.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static FreeSmile.Services.Helper;
using FreeSmile.ActionFilters;
using Swashbuckle.AspNetCore.Annotations;

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
        [ServiceFilter(typeof(ValidUser), Order = 1)]
        [ServiceFilter(typeof(NotSuspended), Order = 2)]
        [ServiceFilter(typeof(VerifiedEmail), Order = 3)]
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


        [Authorize]
        [ServiceFilter(typeof(ValidUser), Order = 1)]
        [ServiceFilter(typeof(NotSuspended), Order = 2)]
        [HttpPut("VerifyAccount")]
        public async Task<IActionResult> VerifyMyAccount([FromBody] OtpDto dto)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
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

        [SwaggerOperation(summary: "either send empty request {} if logged in (to verify email for first time) or send {\"usernameOrEmail\":\"\"} if user forgets password")]
        [HttpPut("RequestOtp")]
        public async Task<IActionResult> RequestEmailOtp([FromBody] RequestOtpDto? value)
        {
            string? user_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(user_id))
            {
                int user_id_int = int.Parse(user_id);
                RegularResponse res = await _userService.RequestEmailOtp(user_id_int);
                return StatusCode(res.StatusCode, res);
            }

            if (!string.IsNullOrEmpty(value?.UsernameOrEmail))
            {
                RegularResponse res = await _userService.RequestEmailOtp(value.UsernameOrEmail);
                return StatusCode(res.StatusCode, res);
            }

            return Unauthorized();
        }

        [HttpPut("ForgotPassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ResetPasswordDto dto)
        {
            var res = await _userService.ChangePassword(dto);
            return StatusCode(res.StatusCode, res);
        }

        [Authorize]
        [ServiceFilter(typeof(ValidUser), Order = 1)]
        [ServiceFilter(typeof(NotSuspended), Order = 2)]
        [ServiceFilter(typeof(VerifiedEmail), Order = 3)]
        [ServiceFilter(typeof(VerifiedIfDentist), Order = 4)]
        [HttpPut("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangeKnownPasswordDto dto)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _userService.ChangePassword(dto, user_id_int);
            return StatusCode(res.StatusCode, res);
        }
        [SwaggerOperation(Summary = "gives error if token expired or suspended user or not verified email or not verified dentist, else it returns the suitable home according to user type for ex (nextpage = \"homeAdmin\")")]
        [ServiceFilter(typeof(ValidUser), Order = 1)]
        [ServiceFilter(typeof(NotSuspended), Order = 2)]
        [ServiceFilter(typeof(VerifiedEmail), Order = 3)]
        [ServiceFilter(typeof(VerifiedIfDentist), Order = 4)]
        [HttpGet("IsAllowedToHome")]
        public async Task<IActionResult> RedirectToHome()
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _userService.RedirectToHome(user_id_int);
            return StatusCode(res.StatusCode, res);
        }
        [SwaggerOperation(summary: "Removes the token Cookie")]
        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete(MyConstants.AUTH_COOKIE_KEY);
            return Ok(
                RegularResponse.Success(
                    message: _localizer["logoutsuccess"],
                    nextPage: Pages.login.ToString()
                ));
        }
    }
}

