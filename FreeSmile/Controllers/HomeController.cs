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
    [Authorize]
    [ServiceFilter(typeof(ValidUser), Order = 1)]
    [ServiceFilter(typeof(NotSuspended), Order = 2)]
    [ServiceFilter(typeof(VerifiedEmail), Order = 3)]
    [ServiceFilter(typeof(VerifiedIfDentist), Order = 4)]
    public class HomeController : ControllerBase
    {
        private readonly IStringLocalizer<HomeController> _localizer;
        private readonly IDentistService _dentistService;
        private readonly IPatientService _patientService;
        private readonly ICommonService _commonService;

        public HomeController(IStringLocalizer<HomeController> localizer, IDentistService dentistService, IPatientService patientService, ICommonService commonService)
        {
            _localizer = localizer;
            _dentistService = dentistService;
            _patientService = patientService;
            _commonService = commonService;
        }

        [HttpPost("AddUpdateReview")]
        public async Task<IActionResult> AddReviewAsync([FromBody] ReviewDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _commonService.AddReviewAsync(value, user_id_int);

            return StatusCode(res.StatusCode, res);
        }
        
        [HttpDelete("DeleteReview")]
        public async Task<IActionResult> DeleteReviewAsync()
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _commonService.DeleteReviewAsync(user_id_int);

            return StatusCode(res.StatusCode, res);
        }
    }
}
