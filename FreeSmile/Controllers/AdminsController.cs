﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using FreeSmile.Services;
using Microsoft.AspNetCore.Authorization;
using FreeSmile.ActionFilters;
using Swashbuckle.AspNetCore.Annotations;
using FreeSmile.DTOs.Admins;
using FreeSmile.DTOs;
using static FreeSmile.Services.Helper;
using static FreeSmile.Services.AuthHelper;
using FreeSmile.CustomValidations;
using FreeSmile.DTOs.Posts;
using FreeSmile.DTOs.Query;
using System.Security.Claims;

namespace FreeSmile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(NotSuspended), Order = 1)]
    [ServiceFilter(typeof(VerifiedEmailTurbo), Order = 2)]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminsController : ControllerBase
    {
        private readonly IStringLocalizer<AdminsController> _localizer;
        private readonly IAdminService _adminService;
        private readonly IDentistService _dentistService;

        public AdminsController(IStringLocalizer<AdminsController> localizer, IAdminService adminService, IDentistService dentistService)
        {
            _localizer = localizer;
            _adminService = adminService;
            _dentistService = dentistService;
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

        [SwaggerOperation(Summary = $"Gets All posts. It returns list of {nameof(GetPostDto)} without dentist info")]
        [HttpGet("GetAllPosts")]
        public async Task<IActionResult> GetAllPosts([FromQuery] PageSize pageSize)
        {
            var res = await _adminService.GetAllPosts(pageSize.Page, pageSize.Size);
            return Ok(res);
        }

        [Authorize(Roles = nameof(Role.SuperAdmin))]
        [SwaggerOperation(Summary = $"Gets Logs for super admin only. It returns List of {nameof(LogDto)}")]
        [HttpGet("GetLogs")]
        public async Task<IActionResult> GetLogsAsync([FromQuery][ValidDate()] string? date)
        {
            var res = await _adminService.GetLogsAsync(date);
            return Ok(res);
        }

        [Authorize(Roles = nameof(Role.SuperAdmin))]
        [SwaggerOperation(Summary = $"Gets Logs for super admin only. It returns List of {nameof(LogSummaryDto)}")]
        [HttpGet("GetLogsSummary")]
        public async Task<IActionResult> GetLogsSummaryAsync([FromQuery][ValidDate()] string? date)
        {
            var res = await _adminService.GetLogsSummaryAsync(date);
            return Ok(res);
        }

        [SwaggerOperation(Summary = $"Takes {nameof(comment_id)} as query and removes that comment if violated")]
        [HttpDelete("ArticleRemoveComment")]
        public async Task<IActionResult> ArticleRemoveCommentAsync([FromQuery] int comment_id)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);
            string role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value!;

            RegularResponse res = await _dentistService.ArticleRemoveCommentAsync(user_id_int, comment_id, role);

            return StatusCode(res.StatusCode, res);
        }

        #region DummyActions
        [HttpPost("Dummy")]
        public void DummyAction2(GetVerificationRequestDto v) { } // Only for including GetVerificationRequestDto in Swagger
        [HttpPost("Dummy2")]
        public void DummyAction3(LogDto v) { } // Only for including LogDto in Swagger
        [HttpPost("Dummy3")]
        public void DummyAction4(LogSummaryDto v) { } // Only for including LogSummaryDto in Swagger
        #endregion

    }
}
