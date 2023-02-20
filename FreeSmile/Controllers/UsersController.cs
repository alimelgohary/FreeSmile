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
            try
            {
                var res = await _patientService.AddUserAsync(value);
                if (string.IsNullOrEmpty(res.Error))
                {
                    var token = AuthHelper.TokenPatient(res.Id, TimeSpan.FromHours(1));
                    Response.Cookies.Append("Authorization-Token", token, new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.Strict, Expires = DateTime.UtcNow.AddHours(1) });
                    return Ok(new { message = _localizer["RegisterSuccess"].ToString(), token });
                }

                return BadRequest(_localizer[res.Error].ToString());
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, _localizer["UnknownError"].ToString());
            }

        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("RegisterAdmin")]
        public async Task<IActionResult> RegisterAdminAsync([FromBody] UserRegisterDto value)
        {
            try
            {
                var res = await _adminService.AddUserAsync(value);
                if (string.IsNullOrEmpty(res.Error))
                {
                    return Ok(new { message = _localizer["RegisterSuccess"].ToString() });
                }

                return BadRequest(_localizer[res.Error].ToString());
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, _localizer["UnknownError"].ToString());
            }

        }


        [HttpPost("RegisterDentist")]
        public async Task<IActionResult> RegisterDentistAsync([FromBody] UserRegisterDto value)
        {
            try
            {
                var res = await _dentistService.AddUserAsync(value);
                if (string.IsNullOrEmpty(res.Error))
                {
                    var token = AuthHelper.TokenDentist(res.Id, TimeSpan.FromHours(1));
                    Response.Cookies.Append("Authorization-Token", token, new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.Strict, Expires = DateTime.UtcNow.AddHours(1) });
                    return Ok(new { message = _localizer["RegisterSuccess"].ToString(), token });
                }
                return BadRequest(_localizer[res.Error].ToString());

            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, _localizer["UnknownError"].ToString());
            }
        }


        [HttpPut("VerifyAccount")]
        [Authorize]
        public async Task<IActionResult> VerifyMyAccount([FromBody]string otp)
        {
            try
            {
                string? user_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(user_id))
                    return Unauthorized();

                int user_id_int = int.Parse(user_id);
                ResponseDTO res = await _userService.VerifyAccount(otp, user_id_int);

                if (string.IsNullOrEmpty(res.Error))
                {
                    return Ok(new { message = _localizer["EmailVerificationSuccess"].ToString()});
                }
                return BadRequest(_localizer[res.Error].ToString());
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, _localizer["UnknownError"].ToString());
            }
        }

        
    }
}
