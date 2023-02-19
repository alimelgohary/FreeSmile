using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FreeSmile.Services
{
    public class Helper
    {
        public static string GenerateToken(ClaimsIdentity claims, TimeSpan tokenAge)
        {
            JwtSecurityTokenHandler tokenHandler = new();
            byte[] key = Encoding.ASCII.GetBytes(MyConstants.JWT_SECRET);
            SecurityTokenDescriptor tokenDescriptor = new()
            {

                Subject = claims,
                Expires = DateTime.UtcNow + tokenAge,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
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
        public struct ResponseDTO
        {
            public int Id;
            public string Error;
        }

    }
}
