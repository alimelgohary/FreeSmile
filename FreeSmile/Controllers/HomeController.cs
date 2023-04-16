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

        [SwaggerOperation(Summary = $"Takes {nameof(ReviewDto)} as JSON & Adds or updates user's review")]
        [HttpPost("AddUpdateReview")]
        public async Task<IActionResult> AddUpdateReviewAsync([FromBody] ReviewDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _commonService.AddUpdateReviewAsync(value, user_id_int);

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

        [SwaggerOperation(Summary = $"Takes {nameof(page)}, {nameof(size)} as query & returns user's notifications (returns list of {nameof(GetNotificationDto)}).")]
        [HttpGet("GetNotifications")]
        public async Task<IActionResult> GetNotificationsAsync(int page = 1, int size = 10)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            var notifications = await _commonService.GetNotificationsAsync(user_id_int, page, size);

            return Ok(notifications);
        }

        [SwaggerOperation(Summary = $"Takes {nameof(notification_id)} as a query parameter & marks notification as seen (send this request when notification clicked")]
        [HttpPut("NotificationSeen")]
        public async Task<IActionResult> NotificationSeenAsync(int notification_id)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            await _commonService.NotificationSeenAsync(notification_id, user_id_int);

            return Ok();
        }

        [SwaggerOperation(Summary = $"Takes {nameof(ReportPostDto)} as JSON & reports post")]
        [HttpPost("ReportPost")]
        public async Task<IActionResult> ReportPostAsync([FromBody] ReportPostDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _commonService.ReportPostAsync(value, user_id_int);

            return StatusCode(res.StatusCode, res);
        }

        [SwaggerOperation(Summary = $"Takes a {nameof(blocked_user_id)} as a query parameter & blocks him")]
        [HttpPost("BlockUser")]
        public async Task<IActionResult> BlockUserAsync(int blocked_user_id)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _commonService.BlockUserAsync(user_id_int, blocked_user_id);

            return StatusCode(res.StatusCode, res);
        }

        [SwaggerOperation(Summary = $"Takes a {nameof(unblocked_user_id)} as a query parameter & unblocks him")]
        [HttpPost("UnblockUser")]
        public async Task<IActionResult> UnblockUserAsync(int unblocked_user_id)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _commonService.UnblockUserAsync(user_id_int, unblocked_user_id);

            return StatusCode(res.StatusCode, res);
        }

        [SwaggerOperation(Summary = $"takes {nameof(page)}, {nameof(size)} as query & Gets list of blocked users (returns List of {nameof(BlockedUsersDto)}).")]
        [HttpGet("GetBlockedUsers")]
        public async Task<IActionResult> GetBlockedListAsync(int page = 1, int size = 10)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            List<BlockedUsersDto> blocked_users = await _commonService.GetBlockedListAsync(user_id_int, page, size);

            return Ok(blocked_users);
        }

        [SwaggerOperation(Summary = $"Takes a {nameof(SendMessageDto)} object as JSON & sends a message to the receiver")]
        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessageAsync([FromBody] SendMessageDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _commonService.SendMessageAsync(value, user_id_int);

            return StatusCode(res.StatusCode, res);
        }

        [SwaggerOperation(Summary = $"takes {nameof(receiver_user_id)} as query and returns Messages with him (returns List of {nameof(GetMessageDto)})")]
        [HttpGet("GetChatHistory")]
        public async Task<IActionResult> GetChatHistoryAsync(int receiver_user_id, int page = 1, int size = 10)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            List<GetMessageDto> res = await _commonService.GetChatHistoryAsync(user_id_int, receiver_user_id, page, size);

            return Ok(res);
        }

        [SwaggerOperation(Summary = $"takes {nameof(page)}, {nameof(size)} as query and returns last messages with all people(returns List of {nameof(RecentMessagesDto)})")]
        [HttpGet("GetRecentMessages")]
        public async Task<IActionResult> GetRecentMessagesAsync(int page = 1, int size = 10)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            List<RecentMessagesDto> res = await _commonService.GetRecentMessagesAsync(user_id_int, page, size);

            return Ok(res);
        }

        [SwaggerOperation(Summary = $"Takes {nameof(ProfilePictureDto)} as Form Data & Adds or updates user's profile picture")]
        [HttpPost("AddUpdateProfilePicture")]
        public async Task<IActionResult> AddUpdateProfilePictureAsync([FromForm] ProfilePictureDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _commonService.AddUpdateProfilePictureAsync(value, user_id_int);

            return StatusCode(res.StatusCode, res);
        }

        [SwaggerOperation(Summary = "Deletes user's profile picture")]
        [HttpDelete("DeleteProfilePicture")]
        public async Task<IActionResult> DeleteProfilePictureAsync()
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _commonService.DeleteProfilePictureAsync(user_id_int);

            return StatusCode(res.StatusCode, res);
        }

        [Authorize(Roles = "Patient,Dentist")]
        [SwaggerOperation(Summary = $"takes {nameof(CaseDto)} as Form Data & creates a case (patient or dentist)")]
        [HttpPost("AddCase")]
        public async Task<IActionResult> AddCaseAsync([FromForm] CaseDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _commonService.AddCaseAsync(value, user_id_int);

            return StatusCode(res.StatusCode, res);
        }

        [Authorize(Roles = "Patient,Dentist")]
        [SwaggerOperation(Summary = $"takes {nameof(UpdateCaseDto)} as JSON & updates a case (patient or dentist)")]
        [HttpPut("UpdateCase")]
        public async Task<IActionResult> UpdateCaseAsync([FromBody] UpdateCaseDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _commonService.UpdateCaseAsync(value, user_id_int);

            return StatusCode(res.StatusCode, res);
        }

        [Authorize(Roles = "Patient,Dentist")]
        [SwaggerOperation(Summary = $"takes {nameof(case_post_id)} as query & deletes a case (patient or dentist)")]
        [HttpDelete("DeleteCase")]
        public async Task<IActionResult> DeleteCaseAsync(int case_post_id)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _commonService.DeleteCaseAsync(user_id_int, case_post_id);

            return StatusCode(res.StatusCode, res);
        }

        #region DummyActions
        [HttpPost("Dummy1")]
        public void Dummy1(GetNotificationDto v) { } // Only for including NotificationDto in Swagger schemas
        [HttpPost("Dummy2")]
        public void Dummy2(BlockedUsersDto v) { } // Only for including BlockedUsersDto in Swagger schemas
        [HttpPost("Dummy3")]
        public void Dummy3(GetMessageDto v) { } // Only for including GetMessageDto in Swagger schemas
        [HttpPost("Dummy4")]
        public void Dummy4(RecentMessagesDto v) { } // Only for including RecentMessagesDto in Swagger schemas
        [HttpPost("Dummy5")]
        public void Dummy5(ProfilePictureDto v) { } // Only for including ProfilePictureDto in Swagger schemas
        [HttpPost("Dummy6")]
        public void Dummy6(CaseDto v) { } // Only for including CaseDto in Swagger schemas
        #endregion

    }
}
