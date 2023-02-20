namespace FreeSmile.Services
{
    public class MyConstants
    {
        public static string JWT_SECRET { get; } = Helper.GetEnvVariable("Jwt_Secret", false);
        public static string PEPPER { get; } = Helper.GetEnvVariable("PEPPER", true);
        public static string FREESMILE_CONNECTION { get; } = Helper.GetEnvVariable("FreeSmileDatabase", true);
        
        public static TimeSpan REGISTER_TOKEN_AGE= TimeSpan.FromHours(1);
        public static TimeSpan LOGIN_TOKEN_AGE = TimeSpan.FromDays(1);
        public const string AUTH_COOKIE_KEY = "Authorization-Token";
    }
}
