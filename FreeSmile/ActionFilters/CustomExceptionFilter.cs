using FreeSmile.Controllers;
using FreeSmile.Models;
using FreeSmile.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Claims;
using static FreeSmile.Services.Helper;

namespace FreeSmile.ActionFilters
{
    public class CustomExceptionFilter : ExceptionFilterAttribute
    {
        private readonly IStringLocalizer<CustomExceptionFilter> _localizer;
        private readonly ILogger<CustomExceptionFilter> _logger;
        public CustomExceptionFilter(ILogger<CustomExceptionFilter> logger, IStringLocalizer<CustomExceptionFilter> localizer)
        {
            _logger = logger;
            _localizer = localizer;
        }
        public override void OnException(ExceptionContext context)
        {
            RegularResponse resObject;
            
            if (context.Exception is GeneralException)
            {
                resObject = RegularResponse.BadRequestError(error: context.Exception.Message);
            }
            else
            {
                resObject = RegularResponse.UnknownError(_localizer);
            }

            var result = new ObjectResult(resObject) { StatusCode = resObject.StatusCode};

            _logger.LogError("{Message}", context.Exception.Message);

            context.Result = result;
        }
    }

    internal class GeneralException : Exception
    {
        public GeneralException(string? message) : base(message)
        {
        }
    }
}
