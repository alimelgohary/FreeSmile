using FreeSmile.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using static FreeSmile.Services.AuthHelper;
using static FreeSmile.Services.Helper;

namespace FreeSmile.ActionFilters
{
    public class VerifiedIfDentist : IAsyncActionFilter
    {
        private readonly FreeSmileContext _context;
        private readonly IStringLocalizer<VerifiedIfDentist> _localizer;
        public VerifiedIfDentist(FreeSmileContext context, IStringLocalizer<VerifiedIfDentist> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            Role role = Enum.Parse<Role>(context.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value!);
            int user_id_int = int.Parse(context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            if (role == Role.Dentist)
            {
                var verified = await _context.Dentists
                                    .AsNoTracking()
                                    .Select(x => new { x.DentistId, x.IsVerifiedDentist })
                                    .FirstOrDefaultAsync(x => x.DentistId == user_id_int);
                
                if (!verified!.IsVerifiedDentist) // Will throw null exception if a dentist keeps his token after deleting his account but I don't care
                {
                    string nextPage;
                    string error;
                    if (await _context.VerificationRequests.AnyAsync(request => request.OwnerId == user_id_int))
                    {
                        nextPage = Pages.pendingVerificationAcceptance.ToString();
                        error = _localizer["pendingverificationacceptance"];
                    }
                    else
                    {
                        nextPage = Pages.verifyDentist.ToString();
                        error = _localizer["VerifyDentistFirst"];
                    }
                    var res = RegularResponse.BadRequestError(error: error, nextPage: nextPage);
                    if (context.HttpContext.Items.TryGetValue(MyConstants.AUTH_COOKIE_KEY, out object? value))
                    {
                        // If token is set by previous filter, pass it
                        res.Token = value!.ToString();
                        TimeSpan loginTokenAge = MyConstants.LOGIN_TOKEN_AGE;
                        context.HttpContext.Response.Cookies.Append(MyConstants.AUTH_COOKIE_KEY, value.ToString()!, new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.None, MaxAge = loginTokenAge, Secure = true });
                    }
                    context.Result = new ObjectResult(res) { StatusCode = res.StatusCode };
                    return;
                }
            }
            if (context.HttpContext.User.FindFirst("verifiedDentist")?.Value! == false.ToString())
            {
                // Is a NEWLY Verified Dentist
                TimeSpan loginTokenAge = MyConstants.LOGIN_TOKEN_AGE;
                string token = GetToken(user_id_int, loginTokenAge, role, true, true);
                if (!context.HttpContext.Items.ContainsKey(MyConstants.AUTH_COOKIE_KEY))
                    context.HttpContext.Items.Add(MyConstants.AUTH_COOKIE_KEY, token);
                else
                    context.HttpContext.Items[MyConstants.AUTH_COOKIE_KEY] = token;
            }
            var result = await next();
        }
    }
}
