using FreeSmile.Models;
using FreeSmile.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using static FreeSmile.Services.Helper;

namespace FreeSmile.ActionFilters
{
    public class ValidUser : IAsyncActionFilter
    {
        private readonly FreeSmileContext _context;
        private readonly IStringLocalizer<ControllerBase> _localizer;
        public ValidUser(FreeSmileContext context, IStringLocalizer<ControllerBase> localizer)
        {
            _context = context;
            _localizer = localizer;
        }


        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            string? user_id = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(user_id)
                || !int.TryParse(user_id, out int user_id_int)
                || !await _context.Users.AnyAsync(user => user.Id == user_id_int))
            {
                RegularResponse res = RegularResponse.BadRequestError(
                                         error: _localizer["UserNotFound"],
                                         nextPage: Pages.login.ToString()
                                      );
                context.Result = new ObjectResult(res) { StatusCode = res.StatusCode };
                return;
            }
            var result = await next();
        }
    }
}
