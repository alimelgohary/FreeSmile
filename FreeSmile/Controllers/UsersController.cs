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
                return BadRequest(_localizer["RegisterError"].ToString());
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
                return BadRequest(_localizer["RegisterError"].ToString());
            }
        }
        [HttpPost("Dummy")]
        public void DummyAction(VerificationDto value){} // Only for including DentistDto in the docs
    }
}
