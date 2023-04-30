using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using FreeSmile.DTOs;
using FreeSmile.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static FreeSmile.Services.Helper;
using FreeSmile.ActionFilters;
using Swashbuckle.AspNetCore.Annotations;
using DTOs;

namespace FreeSmile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(ValidUser), Order = 1)]
    [ServiceFilter(typeof(NotSuspended), Order = 2)]
    [ServiceFilter(typeof(VerifiedEmail), Order = 3)]
    [ServiceFilter(typeof(VerifiedIfDentist), Order = 4)]
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

        [SwaggerOperation(Summary = $"Gets Dentist profile settings in the form of {nameof(GetDentistSettingsDto)}")]
        [HttpGet("GetSettings")]
        public async Task<IActionResult> GetSettingsAsync()
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            var res = await _dentistService.GetSettingsAsync(user_id_int);

            return Ok(res);
        }


        #region DummyActions
        [HttpPost("Dummy")]
        public void DummyAction2(VerificationDto v) { } // Only for including VerificationDto in Swagger
        [HttpPost("Dummy2")]
        public void DummyAction3(RegularResponse r) { } // Only for including General api response in Swagger
        [HttpPost("Dummy3")]
        public void DummyAction4(GetDentistSettingsDto r) { } // Only for including GetDentistSettingsDto in Swagger
        #endregion

    }
}
