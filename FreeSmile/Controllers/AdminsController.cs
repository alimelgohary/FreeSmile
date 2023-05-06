using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using FreeSmile.Services;
using Microsoft.AspNetCore.Authorization;
using FreeSmile.ActionFilters;
using Swashbuckle.AspNetCore.Annotations;
using FreeSmile.DTOs.Admins;
using FreeSmile.DTOs;
using static FreeSmile.Services.Helper;

namespace FreeSmile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(ValidUser), Order = 1)]
    [ServiceFilter(typeof(NotSuspended), Order = 2)]
    [ServiceFilter(typeof(VerifiedEmail), Order = 3)]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminsController : ControllerBase
    {
        private readonly IStringLocalizer<AdminsController> _localizer;
        private readonly IAdminService _adminService;

        public AdminsController(IStringLocalizer<AdminsController> localizer, IAdminService adminService)
        {
            _localizer = localizer;
            _adminService = adminService;
        }

        [SwaggerOperation(Summary = $"Gets Number Of Verification Requests. It returns integer")]
        [HttpGet("GetNumberOfVerificationRequests")]
        public async Task<IActionResult> GetNumberOfVerificationRequestsAsync()
        {
            var res = await _adminService.GetNumberOfVerificationRequestsAsync();
            return Ok(res);
        }

        [SwaggerOperation(Summary = $"Takes integer {nameof(number)} as query and returns a verification request (returns object of {nameof(GetVerificationRequestDto)}) or returns 404 not found error")]
        [HttpGet("GetVerificationRequest")]
        public async Task<IActionResult> GetVerificationRequestAsync(int number = 1)
        {
            var result = await _adminService.GetVerificationRequestAsync(number);
            return Ok(result);
        }

        [SwaggerOperation(Summary = $"Takes {nameof(dentistId.Id)}, {nameof(rejectReasonDto.rejectReason)} as Query & Rejects dentist verification request and emails reason to dentist (1: Incorrect_Info, 2: Photos_Not_Clear, 3: Missing_Photos). This should return {nameof(RegularResponse)} with a success message")]
        [HttpPut("RejectVerificationRequest")]
        public async Task<IActionResult> RejectVerificationRequestAsync([FromQuery] DentistId dentistId, [FromQuery] RejectReasonDto rejectReasonDto)
        {
            var result = await _adminService.RejectVerificationRequestAsync((int)dentistId.Id!, (int)rejectReasonDto.rejectReason!);
            return Ok(result);
        }

        [SwaggerOperation(Summary = $"Takes {nameof(dentistId.Id)} as Query & Accepts dentist verification request and emails success message to dentist. This should return {nameof(RegularResponse)} with a success message")]
        [HttpPut("AcceptVerificationRequest")]
        public async Task<IActionResult> AcceptVerificationRequestAsync([FromQuery] DentistId dentistId)
        {
            var result = await _adminService.AcceptVerificationRequestAsync((int)dentistId.Id!);
            return Ok(result);
        }
        #region DummyActions
        [HttpPost("Dummy")]
        public void DummyAction2(GetVerificationRequestDto v) { } // Only for including GetVerificationRequestDto in Swagger
        #endregion

    }
}
