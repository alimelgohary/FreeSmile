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
    [Authorize(Roles = "Dentist")]
    public class DentistsController : ControllerBase
    {
        private readonly IStringLocalizer<DentistsController> _localizer;
        private readonly IDentistService _dentistService;

        public DentistsController(IStringLocalizer<DentistsController> localizer, IDentistService dentistService)
        {
            _localizer = localizer;
            _dentistService = dentistService;
        }
        
        [HttpPost("RequestVerification")]
        public async Task<IActionResult> AddVerificationRequestAsync([FromForm] VerificationDto value)
        {
            try
            {
                string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if(string.IsNullOrEmpty(userId))
                    return Unauthorized();

                int userIdInt = int.Parse(userId);
                ResponseDTO res = await _dentistService.AddVerificationRequestAsync(value, userIdInt);

                if (string.IsNullOrEmpty(res.Error))
                    return Ok(_localizer["VerificationRequestSuccess"].ToString());

                return BadRequest(_localizer[res.Error].ToString());
            }
            catch (Exception)
            {
                return BadRequest(_localizer["UnknownError"].ToString());
            }
        }
        
        
        [HttpPost("Dummy")]
        public void DummyAction(VerificationDto v){} // Only for including VerificationDto in Swagger
    }
}
