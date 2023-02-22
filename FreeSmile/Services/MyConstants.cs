namespace FreeSmile.Services
{
    public class MyConstants
    {
        public static string FREESMILE_CONNECTION { get; } = Helper.GetEnvVariable("FreeSmileDatabase", true);
        
        public static TimeSpan REGISTER_TOKEN_AGE= TimeSpan.FromHours(1);
        public static TimeSpan LOGIN_TOKEN_AGE = TimeSpan.FromDays(1);
        public const string AUTH_COOKIE_KEY = "Authorization-Token";
        public static TimeSpan OTP_AGE= TimeSpan.FromMinutes(10);
    }
}
