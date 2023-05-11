using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using static FreeSmile.Services.Helper;

namespace FreeSmile.ActionFilters
{
    public class VerifiedIfDentistTurbo : IAsyncActionFilter
    {
        private readonly IStringLocalizer<VerifiedIfDentistTurbo> _localizer;
        public VerifiedIfDentistTurbo(IStringLocalizer<VerifiedIfDentistTurbo> localizer)
        {
            _localizer = localizer;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.User.FindFirst("verifiedDentist")?.Value! == false.ToString())
            {
                var res = RegularResponse.BadRequestError(nextPage: Pages.verifyDentist.ToString(),
                                                          error: _localizer["VerifyDentistFirst"]);

                context.Result = new ObjectResult(res) { StatusCode = res.StatusCode };
                return;
            }
            var result = await next();
        }
    }
}
