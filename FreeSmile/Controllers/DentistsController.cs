using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using FreeSmile.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static FreeSmile.Services.Helper;
using FreeSmile.ActionFilters;
using Swashbuckle.AspNetCore.Annotations;
using FreeSmile.DTOs.Settings;
using FreeSmile.DTOs.Auth;
using FreeSmile.DTOs.Posts;
using FreeSmile.DTOs;

namespace FreeSmile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(NotSuspended), Order = 1)]
    [ServiceFilter(typeof(VerifiedEmailTurbo), Order = 2)]
    [ServiceFilter(typeof(VerifiedIfDentistTurbo), Order = 3)]
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

        [SwaggerOperation(Summary = $"Takes a {nameof(SetDentistSettingsDto)} as JSON and updates Dentist profile settings. Send new values of each field, DO NOT SEND USERNAME IF IT'S NOT UPDATED. It returns updated settings as {nameof(GetDentistSettingsDto)}")]
        [HttpPut("UpdateSettings")]
        public async Task<IActionResult> UpdateSettingsAsync([FromBody] SetDentistSettingsDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            var res = await _dentistService.UpdateSettingsAsync(value, user_id_int);

            return Ok(res);
        }

        [SwaggerOperation(Summary = $"Takes {nameof(pageSize.Page)}, {nameof(pageSize.Size)}, {nameof(gov_id.GovernorateId)}, {nameof(caseTypeDto.CaseTypeId)} as query and returns patients cases in the requested governorate, and caseType. DEFAULT is dentist's university location and all types. (returns List of {nameof(GetCaseDto)})")]
        [HttpGet("GetPatientsCases")]
        public async Task<IActionResult> GetPatientsCasesAsync([FromQuery] PageSize pageSize, [FromQuery] GovernorateDto gov_id, [FromQuery] CaseTypeDto caseTypeDto)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            List<GetCaseDto> result = await _dentistService.GetPatientsCasesAsync(user_id_int, pageSize.Page, pageSize.Size, gov_id.GovernorateId, caseTypeDto.CaseTypeId);

            return Ok(result);
        }

        [SwaggerOperation(Summary = $"Takes {nameof(AddSharingDto)} as Form Data & creates a sharing patient post. GovernorateId is optional. Default is Dentist's current university location. This should return integer created post id if Success")]
        [HttpPost("AddSharing")]
        public async Task<IActionResult> AddSharingAsync([FromForm] AddSharingDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);
            int res = await _dentistService.AddSharingAsync(value, user_id_int);

            return Ok(res);
        }

        [SwaggerOperation(Summary = $"Takes {nameof(AddListingDto)} as Form Data & creates a listing post (product). GovernorateId is optional. Default is Dentist's current university location. This should return integer created post id if Success")]
        [HttpPost("AddListing")]
        public async Task<IActionResult> AddListingAsync([FromForm] AddListingDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);
            int res = await _dentistService.AddListingAsync(value, user_id_int);

            return Ok(res);
        }

        [SwaggerOperation(Summary = $"Takes {nameof(AddArticleDto)} as Form Data & creates an article. This should return integer created post id if Success")]
        [HttpPost("AddArticle")]
        public async Task<IActionResult> AddArticleAsync([FromForm] AddArticleDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);
            int res = await _dentistService.AddArticleAsync(value, user_id_int);

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
