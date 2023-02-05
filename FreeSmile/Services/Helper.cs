using Microsoft.Extensions.Options;

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
                Environment.Exit(1);
            }
            return value;
        }
    }
}
