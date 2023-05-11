using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using static FreeSmile.Services.Helper;

namespace FreeSmile.ActionFilters
{
    public class VerifiedEmailTurbo : IAsyncActionFilter
    {
        private readonly IStringLocalizer<VerifiedEmailTurbo> _localizer;
        public VerifiedEmailTurbo(IStringLocalizer<VerifiedEmailTurbo> localizer)
        {
            _localizer = localizer;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.User.FindFirst("verifiedEmail")?.Value! == false.ToString())
            {
                var res = RegularResponse.BadRequestError(error: _localizer["VerifyEmailFirst"],
                                                          nextPage: Pages.verifyEmail.ToString());

                context.Result = new ObjectResult(res) { StatusCode = res.StatusCode };
                return;
            }
            var result = await next();
        }
    }
}
