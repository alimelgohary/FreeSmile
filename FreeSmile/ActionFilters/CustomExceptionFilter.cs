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
            
            if (context.Exception is GeneralException exception)
            {
                resObject = RegularResponse.BadRequestError(error: context.Exception.Message, nextPage:exception.NextPage);
            }
            else if (context.Exception is InternalServerException ex)
            {
                resObject = new RegularResponse()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Error = context.Exception.Message,
                    NextPage = ex.NextPage
                };
            }
            else if(context.Exception is NotFoundException e) 
            {
                resObject = new RegularResponse()
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Error = context.Exception.Message,
                    NextPage = e.NextPage
                };
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
        public string NextPage;
        public GeneralException(string? message, string NextPage = "same") : base(message)
        {
            this.NextPage = NextPage;
        }
    }
    internal class InternalServerException : Exception
    {
        public string NextPage;
        public InternalServerException(string? message, string NextPage ="same") : base(message)
        {
            this.NextPage = NextPage;
        }
    }
    internal class NotFoundException : Exception
    {
        public string NextPage;
        public NotFoundException(string? message, string NextPage ="same") : base(message)
        {
            this.NextPage = NextPage;
        }
    }
}
