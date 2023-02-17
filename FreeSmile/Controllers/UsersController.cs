using FreeSmile.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using FreeSmile.DTOs;
using FreeSmile.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualBasic;
using static FreeSmile.Services.Helper;
using Microsoft.AspNetCore.Identity;
using static FreeSmile.Services.Helper.MyConstants;

namespace FreeSmile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IStringLocalizer<UsersController> _localizer;
        //private readonly IUserService _userService;
        private readonly IPatientService _patientService;
        private readonly IDentistService _dentistService;

        public UsersController(IStringLocalizer<UsersController> localizer, IPatientService patientService, IDentistService dentistService)
        {
            _localizer = localizer;
            _patientService = patientService;
            _dentistService = dentistService;
        }
        [HttpPost("RegisterPatient")]
        public async Task<IActionResult> RegisterPatientAsync([FromBody] UserRegisterDto value)
        {
            try
            {
                var res = await _patientService.AddUserAsync(value);
                if (string.IsNullOrEmpty(res.Error))
                {
                    return Ok(_localizer["RegisterSuccess"].ToString());
                }
                return BadRequest(res.Error);
                // TODO : Return a token, cookie

            }
            catch (Exception)
            {
                return BadRequest(_localizer["UnknownError"].ToString());
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
                    return Ok(_localizer["RegisterSuccess"].ToString());
                }
                return BadRequest(res.Error);
                // TODO : Return a token, cookie

            }
            catch (Exception)
            {
                return BadRequest(_localizer["UnknownError"].ToString());
            }
        }
        [Authorize(Roles = "Dentist")]
        [HttpPost("RequestVerification")]
        public async Task<IActionResult> AddVerificationRequestAsync([FromForm] VerificationDto value)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if(userId is null)
                    return Unauthorized();

                await _dentistService.AddVerificationRequestAsync(value, userId);
                return Ok(_localizer["VerificationRequestSuccess"].ToString());

            }
            catch (Exception)
            {
                return BadRequest(_localizer["UnknownError"].ToString());
            }
        }
        [HttpPost("Dummy")]
        public void DummyAction(VerificationDto value){} // Only for including VerificationDto in the docs
    }
}
