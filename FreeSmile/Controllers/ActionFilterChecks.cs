using FreeSmile.Models;
using FreeSmile.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using static FreeSmile.Services.Helper;

namespace FreeSmile.Controllers
{
    public class ActionFilterChecks : IAsyncActionFilter
    {
        private readonly FreeSmileContext _context;
        private readonly IStringLocalizer<ControllerBase> _localizer;
        private readonly IPatientService _patientService;
        private readonly IDentistService _dentistService;
        private readonly IAdminService _adminService;
        private readonly IUserService _userService;
        public ActionFilterChecks(FreeSmileContext context,
                                  IStringLocalizer<ControllerBase> localizer,
                                  IPatientService patientService,
                                  IDentistService dentistService,
                                  IAdminService adminService,
                                  IUserService userService)
        {
            _context = context;
            _localizer = localizer;
            _adminService = adminService;
            _patientService = patientService;
            _dentistService = dentistService;
            _userService = userService;
        }
        

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            string? user_id = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(user_id))
            {
                RegularResponse res = RegularResponse.BadRequestError(
                                         error: _localizer["UserNotFound"],
                                         nextPage: Pages.login.ToString()
                                      );
                context.Result = new ObjectResult(res) { StatusCode = res.StatusCode };
                return;
            }

            int user_id_int = int.Parse(user_id);

            if(! _context.Users.Any(user => user.Id == user_id_int))
            {
                RegularResponse res = RegularResponse.BadRequestError(
                                         error: _localizer["UserNotFound"],
                                         nextPage: Pages.login.ToString()
                                      );
                context.Result = new ObjectResult(res) { StatusCode = res.StatusCode };
                return;
            }
            Dentist? dentist = _context.Dentists.Find(user_id_int);
            if (dentist is not null)
            {
                if (await _dentistService.IsNotSuspended(user_id_int) != true)
                {
                    RegularResponse res = RegularResponse.BadRequestError(
                                             error: _localizer["UserSuspended"]
                                          );

                    context.Result = new ObjectResult(res) { StatusCode = res.StatusCode };
                    return;
                }

                if (await _dentistService.IsVerifiedEmail(user_id_int) != true)
                {
                    RegularResponse res = RegularResponse.BadRequestError(
                                             error: _localizer["VerifyEmailFirst"],
                                             nextPage : Pages.verifyEmail.ToString()
                                          );

                    context.Result = new ObjectResult(res) { StatusCode = res.StatusCode };
                    return;
                }
                if (await _dentistService.IsVerifiedDentist(user_id_int) != true)
                {
                    RegularResponse res = RegularResponse.BadRequestError(
                                             error: _localizer["VerifyDentistFirst"],
                                             nextPage: Pages.verifyDentist.ToString()
                                          );

                    context.Result = new ObjectResult(res) { StatusCode = res.StatusCode };
                    return;
                }
            }
            else
            {
                if (await _userService.IsNotSuspended(user_id_int) != true)
                {
                    RegularResponse res = RegularResponse.BadRequestError(
                                             error: _localizer["UserSuspended"]
                                          );

                    context.Result = new ObjectResult(res) { StatusCode = res.StatusCode };
                    return;
                }

                if (await _userService.IsVerifiedEmail(user_id_int) != true)
                {
                    RegularResponse res = RegularResponse.BadRequestError(
                                             error: _localizer["VerifyEmailFirst"],
                                             nextPage: Pages.verifyEmail.ToString()
                                          );

                    context.Result = new ObjectResult(res) { StatusCode = res.StatusCode };
                    return;
                }
            }

            
            var result = await next();
        }
    }
}
