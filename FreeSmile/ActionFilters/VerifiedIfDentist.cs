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
    public class VerifiedIfDentist : IAsyncActionFilter
    {
        private readonly FreeSmileContext _context;
        private readonly IStringLocalizer<ControllerBase> _localizer;
        public VerifiedIfDentist(FreeSmileContext context, IStringLocalizer<ControllerBase> localizer)
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

            var verified = await _context.Dentists
                                    .AsNoTracking()
                                    .Select(x => new { x.DentistId, x.IsVerifiedDentist })
                                    .FirstOrDefaultAsync(x => x.DentistId == user_id_int);
            if (verified is not null)
            {
                string nextPage;
                if (!verified.IsVerifiedDentist)
                {
                    nextPage = Pages.verifyDentist.ToString();

                    RegularResponse res = RegularResponse.BadRequestError(
                                             error: _localizer["VerifyDentistFirst"],
                                             nextPage: nextPage
                                          );

                    if (await _context.VerificationRequests.AnyAsync(request => request.OwnerId == user_id_int))
                    {
                        nextPage = Pages.pendingVerificationAcceptance.ToString();
                        res = RegularResponse.BadRequestError(
                                             error: _localizer["pendingverificationacceptance"],
                                             nextPage: nextPage
                                          );
                    }


                    context.Result = new ObjectResult(res) { StatusCode = res.StatusCode };
                    return;
                }
            }
            var result = await next();
        }
    }
}
