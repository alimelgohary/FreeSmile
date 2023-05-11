using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using FreeSmile.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FreeSmile.ActionFilters;
using Swashbuckle.AspNetCore.Annotations;
using FreeSmile.DTOs.Settings;
using FreeSmile.DTOs;
using FreeSmile.DTOs.Posts;

namespace FreeSmile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(NotSuspended), Order = 1)]
    [ServiceFilter(typeof(VerifiedEmailTurbo), Order = 2)]
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

        [SwaggerOperation(Summary = $"Takes {nameof(pageSize.Page)}, {nameof(pageSize.Size)}, {nameof(gov_id.GovernorateId)}, {nameof(caseTypeDto.CaseTypeId)} as query and returns dentists cases in the requested governorate, and caseType. DEFAULT is all governorates and all types. (returns List of {nameof(GetCaseDto)})")]
        [HttpGet("GetDentistsCases")]
        public async Task<IActionResult> GetDentistsCasesAsync([FromQuery] PageSize pageSize, [FromQuery] GovernorateDto gov_id, [FromQuery] CaseTypeDto caseTypeDto)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            List<GetCaseDto> result= await _patientService.GetDentistsCases(user_id_int, pageSize.Page, pageSize.Size, gov_id.GovernorateId, caseTypeDto.CaseTypeId);

            return Ok(result);
        }

        #region DummyActions
        [HttpPost("Dummy")]
        public void DummyAction2(GetCaseDto v) { } // Only for including GetCaseDto in Swagger
        #endregion
    }
}
