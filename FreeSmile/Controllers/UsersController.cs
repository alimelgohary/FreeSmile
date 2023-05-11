using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using FreeSmile.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static FreeSmile.Services.Helper;
using FreeSmile.ActionFilters;
using Swashbuckle.AspNetCore.Annotations;
using FreeSmile.DTOs.Auth;

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
            var res = await _userService.AddPatientAsync(value, Response.Cookies);
            return StatusCode(res.StatusCode, res);
        }


        [Authorize(Roles = "SuperAdmin")]
        [ServiceFilter(typeof(NotSuspended), Order = 1)]
        [ServiceFilter(typeof(VerifiedEmail), Order = 2)]
        [HttpPost("RegisterAdmin")]
        public async Task<IActionResult> RegisterAdminAsync([FromBody] UserRegisterDto value)
        {
            var res = await _userService.AddAdminAsync(value, Response.Cookies);
            return StatusCode(res.StatusCode, res);
        }


        [HttpPost("RegisterDentist")]
        public async Task<IActionResult> RegisterDentistAsync([FromBody] UserRegisterDto value)
        {
            var res = await _userService.AddDentistAsync(value, Response.Cookies);
            return StatusCode(res.StatusCode, res);
        }


        [Authorize]
        [ServiceFilter(typeof(NotSuspended), Order = 1)]
        [HttpPut("VerifyAccount")]
        public async Task<IActionResult> VerifyMyAccount([FromBody] OtpDto dto)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);
            string role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value!;

            RegularResponse res = await _userService.VerifyAccount(dto.Otp, user_id_int, role, Response.Cookies);
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
                string role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value!;
                RegularResponse res = await _userService.RequestEmailOtp(user_id_int, role);
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
        [ServiceFilter(typeof(NotSuspended), Order = 1)]
        [ServiceFilter(typeof(VerifiedEmailTurbo), Order = 2)]
        [ServiceFilter(typeof(VerifiedIfDentistTurbo), Order = 3)]
        [HttpPut("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangeKnownPasswordDto dto)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _userService.ChangePassword(dto, user_id_int);
            return StatusCode(res.StatusCode, res);
        }

        [SwaggerOperation(Summary = "Slow verison, MUST use in verify email, verify dentist and waiting pages gives error if token expired or suspended user or not verified email or not verified dentist, else it returns the suitable home according to user type for ex (nextpage = \"homeAdmin\")")]
        [Authorize]
        [ServiceFilter(typeof(NotSuspended), Order = 1)]
        [ServiceFilter(typeof(VerifiedEmail), Order = 2)]
        [ServiceFilter(typeof(VerifiedIfDentist), Order = 3)]
        [HttpGet("IsAllowedToHome")]
        public IActionResult RedirectToHome()
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);
            string role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value!;

            RegularResponse res = _userService.RedirectToHome(user_id_int, role);
            if (HttpContext.Items.TryGetValue(MyConstants.AUTH_COOKIE_KEY, out object? value))
            {
                // This gets a new token with VerifiedDentist=True
                // When user is waiting for verification and an admin verified him, so when calling this action, the old cookie is replaced by new cookie
                res.Token = value!.ToString();
                TimeSpan loginTokenAge = MyConstants.LOGIN_TOKEN_AGE;
                Response.Cookies.Append(MyConstants.AUTH_COOKIE_KEY, value.ToString()!, new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.None, MaxAge = loginTokenAge, Secure = true });
            }
            return StatusCode(res.StatusCode, res);
        }

        [SwaggerOperation(Summary = "TURBOOOOOOOOOOOOOO gives error if token expired or suspended user or not verified email or not verified dentist, else it returns the suitable home according to user type for ex (nextpage = \"homeAdmin\")")]
        [Authorize]
        [ServiceFilter(typeof(NotSuspended), Order = 1)]
        [ServiceFilter(typeof(VerifiedEmailTurbo), Order = 2)]
        [ServiceFilter(typeof(VerifiedIfDentistTurbo), Order = 3)]
        [HttpGet("IsAllowedToHomeTurbo")]
        public IActionResult RedirectToHomeTurbo()
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);
            string role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value!;

            RegularResponse res = _userService.RedirectToHome(user_id_int, role);
            return StatusCode(res.StatusCode, res);
        }
        
        [SwaggerOperation(summary: "Removes the token Cookie")]
        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Append(MyConstants.AUTH_COOKIE_KEY, string.Empty, new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.None, Expires = DateTime.Now.AddDays(-5), Secure = true });
            return Ok(
                RegularResponse.Success(
                    message: _localizer["logoutsuccess"],
                    nextPage: Pages.login.ToString()
                )
            );
        }

        [SwaggerOperation(summary: "Takes your current password and deletes your account")]
        [Authorize]
        [ServiceFilter(typeof(NotSuspended), Order = 1)]
        [HttpDelete("DeleteMyAccount")]
        public async Task<IActionResult> DeleteMyAccount([FromBody] DeleteMyAccountDto value)
        {
            string? user_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int user_id_int = int.Parse(user_id!);
            RegularResponse res = await _userService.DeleteMyAccount(value, user_id_int, Response.Cookies);
            return StatusCode(res.StatusCode, res);
            
        }

        [SwaggerOperation(Summary = "content type: multipart/form-data, lets dentist send national id photo, proof photo to be checked by an admin for verification")]
        [ServiceFilter(typeof(NotSuspended), Order = 1)]
        [ServiceFilter(typeof(VerifiedEmailTurbo), Order = 2)]
        [Authorize(Roles = "Dentist")]
        [HttpPost("RequestVerification")]
        public async Task<IActionResult> AddVerificationRequestAsync([FromForm] VerificationDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _dentistService.AddVerificationRequestAsync(value, user_id_int);

            return StatusCode(res.StatusCode, res);
        }

        [SwaggerOperation(Summary = $"Deletes dentist's verification request. Should return {nameof(RegularResponse)} with a success message")]
        [ServiceFilter(typeof(NotSuspended), Order = 1)]
        [ServiceFilter(typeof(VerifiedEmailTurbo), Order = 2)]
        [Authorize(Roles = "Dentist")]
        [HttpDelete("DeleteDentistVerificationRequest")]
        public async Task<IActionResult> DeleteVerificationRequestAsync()
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _dentistService.DeleteVerificationRequestAsync(user_id_int);

            return StatusCode(res.StatusCode, res);
        }
    }
}

