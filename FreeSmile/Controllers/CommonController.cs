using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using FreeSmile.DTOs;
using FreeSmile.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static FreeSmile.Services.Helper;
using FreeSmile.ActionFilters;
using Swashbuckle.AspNetCore.Annotations;
using FreeSmile.DTOs.Posts;

namespace FreeSmile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ServiceFilter(typeof(NotSuspended), Order = 1)]
    [ServiceFilter(typeof(VerifiedEmailTurbo), Order = 2)]
    [ServiceFilter(typeof(VerifiedIfDentistTurbo), Order = 3)]
    public class CommonController : ControllerBase
    {
        private readonly IStringLocalizer<CommonController> _localizer;
        private readonly IDentistService _dentistService;
        private readonly IPatientService _patientService;
        private readonly ICommonService _commonService;

        public CommonController(IStringLocalizer<CommonController> localizer, IDentistService dentistService, IPatientService patientService, ICommonService commonService)
        {
            _localizer = localizer;
            _dentistService = dentistService;
            _patientService = patientService;
            _commonService = commonService;
        }

        [SwaggerOperation(Summary = $"Gets authenticated user's review. This Should return {nameof(ReviewDto)}")]
        [HttpGet("GetReview")]
        public async Task<IActionResult> GetReviewAsync()
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            ReviewDto review = await _commonService.GetReviewAsync(user_id_int);

            return Ok(review);
        }

        [SwaggerOperation(Summary = $"Takes {nameof(ReviewDto)} as JSON & Adds or updates user's review. This should return {nameof(RegularResponse)} with a success message")]
        [HttpPost("AddUpdateReview")]
        public async Task<IActionResult> AddUpdateReviewAsync([FromBody] ReviewDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _commonService.AddUpdateReviewAsync(value, user_id_int);

            return StatusCode(res.StatusCode, res);
        }

        [SwaggerOperation(Summary = $"Deletes user's review. This should return {nameof(RegularResponse)} with a success message")]
        [HttpDelete("DeleteReview")]
        public async Task<IActionResult> DeleteReviewAsync()
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _commonService.DeleteReviewAsync(user_id_int);

            return StatusCode(res.StatusCode, res);
        }

        [SwaggerOperation(Summary = $"Takes {nameof(pageSize.Page)}, {nameof(pageSize.Size)}  as query & returns user's notifications (returns list of {nameof(GetNotificationDto)}).")]
        [HttpGet("GetNotifications")]
        public async Task<IActionResult> GetNotificationsAsync([FromQuery] PageSize pageSize)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            var notifications = await _commonService.GetNotificationsAsync(user_id_int, pageSize.Page, pageSize.Size);

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

        [SwaggerOperation(Summary = $"Takes {nameof(ReportPostDto)} as JSON & reports post. This should return {nameof(RegularResponse)} with a success message")]
        [HttpPost("ReportPost")]
        public async Task<IActionResult> ReportPostAsync([FromBody] ReportPostDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _commonService.ReportPostAsync(value, user_id_int);

            return StatusCode(res.StatusCode, res);
        }

        [SwaggerOperation(Summary = $"Takes a {nameof(blocked_user_id)} as a query parameter & blocks him. This should return {nameof(RegularResponse)} with a success message")]
        [HttpPost("BlockUser")]
        public async Task<IActionResult> BlockUserAsync(int blocked_user_id)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _commonService.BlockUserAsync(user_id_int, blocked_user_id);

            return StatusCode(res.StatusCode, res);
        }

        [SwaggerOperation(Summary = $"Takes a {nameof(unblocked_user_id)} as a query parameter & unblocks him. This should return {nameof(RegularResponse)} with a success message")]
        [HttpPost("UnblockUser")]
        public async Task<IActionResult> UnblockUserAsync(int unblocked_user_id)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _commonService.UnblockUserAsync(user_id_int, unblocked_user_id);

            return StatusCode(res.StatusCode, res);
        }

        [SwaggerOperation(Summary = $"Takes {nameof(pageSize.Page)}, {nameof(pageSize.Size)} as query & Gets list of blocked users (returns List of {nameof(GetBlockedUsersDto)}).")]
        [HttpGet("GetBlockedUsers")]
        public async Task<IActionResult> GetBlockedListAsync([FromQuery] PageSize pageSize)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            List<GetBlockedUsersDto> blocked_users = await _commonService.GetBlockedListAsync(user_id_int, pageSize.Page, pageSize.Size);

            return Ok(blocked_users);
        }
        
        [SwaggerOperation(Summary = $"Takes a {nameof(SendMessageDto)} object as JSON & sends a message to the receiver. This should return {nameof(GetMessageDto)}")]
        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessageAsync([FromBody] SendMessageDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            GetMessageDto res = await _commonService.SendMessageAsync(value, user_id_int);

            return Ok(res);
        }

        [SwaggerOperation(Summary = $"Takes {nameof(receiver_user_id)}, {nameof(pageSize.Page)}, {nameof(pageSize.Size)}, {nameof(after)} (to retrieve only messages after given id) as query and returns Messages with him (returns List of {nameof(GetMessageDto)}) sorted DESC (recent messages first)")]
        [HttpGet("GetChatHistory")]
        public async Task<IActionResult> GetChatHistoryAsync(int receiver_user_id, [FromQuery]PageSize pageSize, [FromQuery] int after)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);
            
            List<GetMessageDto> res = await _commonService.GetChatHistoryAsync(user_id_int, receiver_user_id, pageSize.Page, pageSize.Size, after);

            return Ok(res);
        }

        [SwaggerOperation(Summary = $"Takes {nameof(pageSize.Page)}, {nameof(pageSize.Size)}, {nameof(q)} (query search last messages) as query and returns last messages with all people(returns List of {nameof(RecentMessagesDto)})")]
        [HttpGet("GetRecentMessages")]
        public async Task<IActionResult> GetRecentMessagesAsync([FromQuery] PageSize pageSize, [FromQuery] string? q)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            List<RecentMessagesDto> res = await _commonService.GetRecentMessagesAsync(user_id_int, pageSize.Page, pageSize.Size, q);

            return Ok(res);
        }

        [SwaggerOperation(Summary = $"Takes {nameof(AddProfilePictureDto)} as Form Data & Adds or updates user's profile picture, returns the same picture as base64 in a 100x100 size if success")]
        [HttpPost("AddUpdateProfilePicture")]
        public async Task<IActionResult> AddUpdateProfilePictureAsync([FromForm] AddProfilePictureDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            byte[] res = await _commonService.AddUpdateProfilePictureAsync(value, user_id_int);

            return Ok(res);
        }

        [SwaggerOperation(Summary = $"Deletes user's profile picture. This should return {nameof(RegularResponse)} with a success message")]
        [HttpDelete("DeleteProfilePicture")]
        public IActionResult DeleteProfilePictureAsync()
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = _commonService.DeleteProfilePictureAsync(user_id_int);

            return StatusCode(res.StatusCode, res);
        }

        [Authorize(Roles = "Patient,Dentist")]
        [SwaggerOperation(Summary = $"Takes {nameof(AddCaseDto)} as Form Data & creates a case (patient or dentist). GovernorateId is required if patient. and ignored if dentist. This should return integer created post id if Success")]
        [HttpPost("AddCase")]
        public async Task<IActionResult> AddCaseAsync([FromForm] AddCaseDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);
            string role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value!;
            int res = await _commonService.AddCaseAsync(value, user_id_int, role);

            return Ok(res);
        }

        [Authorize(Roles = "Patient,Dentist")]
        [SwaggerOperation(Summary = $"Takes {nameof(UpdateCaseDto)} as JSON & updates a case (patient or dentist). This should return {nameof(RegularResponse)} with a success message")]
        [HttpPut("UpdateCase")]
        public async Task<IActionResult> UpdateCaseAsync([FromBody] UpdateCaseDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);
            string role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value!;

            RegularResponse res = await _commonService.UpdateCaseAsync(value, user_id_int, role);

            return StatusCode(res.StatusCode, res);
        }

        [SwaggerOperation(Summary = $"Takes {nameof(post_id)} as query & deletes a post of any type (case-patient or case-dentist or sharing or listing or article). Admin also can delete any violated post. This should return {nameof(RegularResponse)} with a success message")]
        [HttpDelete("DeletePost")]
        public async Task<IActionResult> DeleteAnyPostTypeAsync(int post_id)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);
            string role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value!;

            RegularResponse res = await _commonService.DeletePostAsync(user_id_int, post_id, role);
            
            return StatusCode(res.StatusCode, res);
        }

        #region DummyActions
        [HttpPost("Dummy1")]
        public void Dummy1(GetNotificationDto v) { } // Only for including NotificationDto in Swagger schemas
        [HttpPost("Dummy2")]
        public void Dummy2(GetBlockedUsersDto v) { } // Only for including BlockedUsersDto in Swagger schemas
        [HttpPost("Dummy3")]
        public void Dummy3(GetMessageDto v) { } // Only for including GetMessageDto in Swagger schemas
        [HttpPost("Dummy4")]
        public void Dummy4(RecentMessagesDto v) { } // Only for including RecentMessagesDto in Swagger schemas
        [HttpPost("Dummy5")]
        public void Dummy5(AddProfilePictureDto v) { } // Only for including ProfilePictureDto in Swagger schemas
        [HttpPost("Dummy6")]
        public void Dummy6(AddCaseDto v) { } // Only for including CaseDto in Swagger schemas
        [HttpPost("Dummy7")]
        public void Dummy7(PageSize v) { } // Only for including PageSize in Swagger schemas
        #endregion

    }
}
