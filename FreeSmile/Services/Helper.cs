using Microsoft.Extensions.Options;

namespace FreeSmile.Services
{
    public partial class Helper
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
        public struct ResponseDTO
        {
            public int Id;
            public string Error;
        }

    }
}
