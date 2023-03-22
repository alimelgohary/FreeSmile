using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using FreeSmile.DTOs;
using FreeSmile.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static FreeSmile.Services.Helper;
using FreeSmile.ActionFilters;

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
        [ServiceFilter(typeof(ValidUser), Order = 1)]
        [ServiceFilter(typeof(NotSuspended), Order = 2)]
        [ServiceFilter(typeof(VerifiedEmail), Order = 3)]

        public async Task<IActionResult> AddVerificationRequestAsync([FromForm] VerificationDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _dentistService.AddVerificationRequestAsync(value, user_id_int);

            return StatusCode(res.StatusCode, res);
        }

        #region DummyActions
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("Dummy")]
        public void DummyAction2(VerificationDto v) { } // Only for including VerificationDto in Swagger
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("Dummy2")]
        public void DummyAction3(RegularResponse r) { } // Only for including api response in Swagger
        #endregion

    }
}
