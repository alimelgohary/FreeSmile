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
            
            if (context.Exception is GeneralException e1)
            {
                resObject = RegularResponse.BadRequestError(error: context.Exception.Message, nextPage: e1.NextPage);
            }
            else if (context.Exception is InternalServerException e2)
            {
                resObject = new RegularResponse()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Error = context.Exception.Message,
                    NextPage = e2.NextPage
                };
            }
            else if(context.Exception is NotFoundException e3) 
            {
                resObject = new RegularResponse()
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Error = context.Exception.Message,
                    NextPage = e3.NextPage
                };
            }
            else if(context.Exception is UnauthorizedException e4) 
            {
                resObject = new RegularResponse()
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Error = context.Exception.Message,
                    NextPage = e4.NextPage
                };
            }
            else
            {
                resObject = RegularResponse.UnknownError(_localizer);
            }

            var result = new ObjectResult(resObject) { StatusCode = resObject.StatusCode};

            _logger.LogError("{StackTrace} {Message}", context.Exception.StackTrace, context.Exception.Message);

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
    internal class UnauthorizedException : Exception
    {
        public string NextPage;
        public UnauthorizedException(string? message, string NextPage ="same") : base(message)
        {
            this.NextPage = NextPage;
        }
    }

}
