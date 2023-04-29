using FreeSmile.Models;
using FreeSmile.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using static FreeSmile.Services.Helper;

namespace FreeSmile.ActionFilters
{
    public class NotSuspended : IAsyncActionFilter
    {
        private readonly FreeSmileContext _context;
        private readonly IStringLocalizer<ControllerBase> _localizer;
        public NotSuspended(FreeSmileContext context, IStringLocalizer<ControllerBase> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // I'm sure it's not null because of previous filters
            string user_id = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

            // I'm sure it's int because of previous filters
            int user_id_int = int.Parse(user_id);

            // I'm sure it's a valid user because of previous filters
            var sus = await _context.Users.AsNoTracking()
                                          .Select(x => new { x.Id, x.Suspended })
                                          .FirstOrDefaultAsync(x => x.Id == user_id_int)!;

            if (sus!.Suspended == true)
            {
                RegularResponse res = RegularResponse.BadRequestError(
                                         error: _localizer["UserSuspended"]
                                      );

                context.Result = new ObjectResult(res) { StatusCode = res.StatusCode };
                return;
            }

            var result = await next();
        }
    }
}
