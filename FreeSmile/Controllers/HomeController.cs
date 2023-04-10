using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using FreeSmile.DTOs;
using FreeSmile.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static FreeSmile.Services.Helper;
using FreeSmile.ActionFilters;
using Swashbuckle.AspNetCore.Annotations;

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
        
        [SwaggerOperation(Summary = "Adds or updates user's review")]
        [HttpPost("AddUpdateReview")]
        public async Task<IActionResult> AddReviewAsync([FromBody] ReviewDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _commonService.AddReviewAsync(value, user_id_int);

            return StatusCode(res.StatusCode, res);
        }

        [SwaggerOperation(Summary = "Deletes user's review")]
        [HttpDelete("DeleteReview")]
        public async Task<IActionResult> DeleteReviewAsync()
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _commonService.DeleteReviewAsync(user_id_int);

            return StatusCode(res.StatusCode, res);
        }
        
        [SwaggerOperation(Summary = "Takes page number, size, and returns user's notifications (Check NotificationDto). It returns all notifications if page not specified")]
        [HttpGet("GetNotifications")]
        public async Task<IActionResult> GetNotificationsAsync(int page, int size = 10)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            var notifications = await _commonService.GetNotificationsAsync(user_id_int, page, size);

            return Ok(notifications);
        }

        [SwaggerOperation(Summary = "Takes notification id as a query parameter and marks notification as seen")]
        [HttpPut("NotificationSeen")]
        public async Task<IActionResult> NotificationSeenAsync(int notification_id)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            await _commonService.NotificationSeenAsync(notification_id, user_id_int);

            return Ok();
        }

        #region DummyActions
        [HttpPost("Dummy")]
        public void Dummy1(GetNotificationDto v) { } // Only for including NotificationDto in Swagger
        #endregion

    }
}
