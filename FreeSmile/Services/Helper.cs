using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
        
        public struct RegularResponse
        {
            public int Id { get; set; }
            public string? Token { get; set; }
            public string? Error { get; set; }
            public string? Message { get; set; }
            public string? NextPage { get; set; }
            [JsonIgnore]
            public int StatusCode { get; set; }
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
            same
        }

    }
}
