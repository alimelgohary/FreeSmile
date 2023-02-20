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
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            int userIdInt = int.Parse(userId);
            RegularResponse res = await _dentistService.AddVerificationRequestAsync(value, userIdInt);

            return StatusCode(res.StatusCode, res);
        }

        [HttpPost("Dummy2")]
        public void DummyAction(UserLoginDto v) { }
        [HttpPost("Dummy")]
        public void DummyAction2(VerificationDto v) { } // Only for including VerificationDto in Swagger
    }
}
