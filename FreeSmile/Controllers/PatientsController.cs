using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using FreeSmile.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static FreeSmile.Services.Helper;
using FreeSmile.ActionFilters;
using Swashbuckle.AspNetCore.Annotations;
using DTOs;
using FreeSmile.DTOs.Settings;

namespace FreeSmile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(ValidUser), Order = 1)]
    [ServiceFilter(typeof(NotSuspended), Order = 2)]
    [ServiceFilter(typeof(VerifiedEmail), Order = 3)]
    [Authorize(Roles = "Patient")]
    public class PatientsController : ControllerBase
    {
        private readonly IStringLocalizer<DentistsController> _localizer;
        private readonly IPatientService _patientService;

        public PatientsController(IStringLocalizer<DentistsController> localizer, IPatientService patientService)
        {
            _localizer = localizer;
            _patientService = patientService;
        }

        [SwaggerOperation(Summary = $"Gets Patient profile settings in the form of {nameof(GetPatientSettingsDto)}")]
        [HttpGet("GetSettings")]
        public async Task<IActionResult> GetSettingsAsync()
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            var res = await _patientService.GetSettingsAsync(user_id_int);

            return Ok(res);
        }

        [SwaggerOperation(Summary = $"Takes a {nameof(SetPatientSettingsDto)} as JSON and updates Dentist profile settings. Send new values of each field, DO NOT SEND USERNAME IF IT'S NOT UPDATED. It returns updated settings as {nameof(GetPatientSettingsDto)}")]
        [HttpPut("UpdateSettings")]
        public async Task<IActionResult> UpdateSettingsAsync([FromBody] SetPatientSettingsDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            var res = await _patientService.UpdateSettingsAsync(value, user_id_int);

            return Ok(res);
        }
    }
}
