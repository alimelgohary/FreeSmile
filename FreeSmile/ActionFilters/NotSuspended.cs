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
        private readonly IStringLocalizer<NotSuspended> _localizer;
        public NotSuspended(FreeSmileContext context, IStringLocalizer<NotSuspended> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            string? auth_user_id = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            int user_id_int = 0;
            if (!string.IsNullOrEmpty(auth_user_id))
            {
                user_id_int = int.Parse(auth_user_id);

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
            }
            var result = await next();
        }
    }
}
