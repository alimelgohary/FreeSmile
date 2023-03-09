using Microsoft.Extensions.Localization;
using System.Text.Json.Serialization;

namespace FreeSmile.Services
{
    public class Helper
    {
        public static string? GetEnvVariable(string key, bool closeIfNotFound)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrEmpty(value) && closeIfNotFound) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Fatal Error: {key} is not found in Environment Variables.");
                
                if (closeIfNotFound)
                    Environment.Exit(1);
            }
            return value;
        }

        public async static Task SaveToDisk(IFormFile? file, string path)
        {
            //DO NOT REMOVE THIS CHECK
            if (file is null)
                return;
            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
        }
        
        public class RegularResponse
        {
            public int Id { get; set; }
            public string? Token { get; set; }
            public string? Error { get; set; }
            public string? Message { get; set; }
            public string? NextPage { get; set; }
            [JsonIgnore]
            public int StatusCode { get; set; }

            public static RegularResponse UnknownError(IStringLocalizer _localizer)
            {
                return new RegularResponse()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Error = _localizer["UnknownError"],
                    NextPage = Pages.same.ToString()
                };
            }
            public static RegularResponse Success(int id = 0, string? token = null, string? message = null, string? nextPage = "same")
            {
                return new RegularResponse()
                {
                    Id = id,
                    Token = token,
                    Message = message,
                    NextPage = nextPage,
                    StatusCode = StatusCodes.Status200OK,
                };
            }
            public static RegularResponse BadRequestError(int id = 0, string? error = null, string? nextPage = "same")
            {
                return new RegularResponse()
                {
                    Id = id,
                    Error = error,
                    NextPage = nextPage,
                    StatusCode = StatusCodes.Status400BadRequest,
                };
            }

        }
        
        public enum Pages
        {
            home, // homeAdmin, homeDentist, homePatient, homeSuperAdmin
            login,
            register,
            verifyEmail,
            verifyDentist,
            registerAdmin,
            registerDentist,
            registerPatient,
            pendingVerificationAcceptance,
            same,
            postFullPage // postFullPage{post_id}  postFullPage25 postFullPage5154
        }
        public enum NotificationTemplates
        {
            Incorrect_Info,
            Photos_Not_Clear,
            Missing_Photos,
            Verification_Success,
            Article_Like,
            Article_Comment,
            Reset_Password,
            Changed_Password
        }

    }
}
