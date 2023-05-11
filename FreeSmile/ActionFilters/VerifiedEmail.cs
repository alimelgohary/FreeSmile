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
    public class VerifiedEmail : IAsyncActionFilter
    {
        private readonly FreeSmileContext _context;
        private readonly IStringLocalizer<VerifiedEmail> _localizer;
        public VerifiedEmail(FreeSmileContext context, IStringLocalizer<VerifiedEmail> localizer)
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
            var verified = await _context.Users.AsNoTracking()
                                                .Select(x => new { x.Id, x.IsVerified })
                                                .FirstOrDefaultAsync(x => x.Id == user_id_int)!;

            if (verified!.IsVerified != true)
            {
                var res = RegularResponse.BadRequestError(error: _localizer["VerifyEmailFirst"],
                                                          nextPage: Pages.verifyEmail.ToString());

                context.Result = new ObjectResult(res) { StatusCode = res.StatusCode };
                return;
            }
            if (context.HttpContext.User.FindFirst("verifiedEmail")?.Value! == false.ToString())
            {
                // Is a NEWLY Verified email
                Role role = Enum.Parse<Role>(context.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value!);
                TimeSpan loginTokenAge = MyConstants.LOGIN_TOKEN_AGE;
                string token = GetToken(user_id_int, loginTokenAge, role, verifiedEmail: true);
                context.HttpContext.Items.Add(MyConstants.AUTH_COOKIE_KEY, token);
                if (!context.HttpContext.Items.ContainsKey(MyConstants.AUTH_COOKIE_KEY))
                    context.HttpContext.Items.Add(MyConstants.AUTH_COOKIE_KEY, token);
                else
                    context.HttpContext.Items[MyConstants.AUTH_COOKIE_KEY] = token;
            }
            var result = await next();
        }
    }
}
