namespace FreeSmile.Services
{
    public class MyConstants
    {
        public static string FREESMILE_CONNECTION { get; } = Helper.GetEnvVariable("_FreeSmileDatabase");

        public static TimeSpan REGISTER_TOKEN_AGE = TimeSpan.FromHours(1);
        public static TimeSpan LOGIN_TOKEN_AGE = TimeSpan.FromDays(1);
        public const string AUTH_COOKIE_KEY = "Authorization-Token";
        public static TimeSpan OTP_AGE = TimeSpan.FromMinutes(10);
        public const string otpemailfilename = "otpemail.html";
        public const string IMAGES_PATH = "Images";

        //"Images\profilePics\{id[0]}\{id}\{size}"
        public static string GetProfilePicturesUser(int id) => Path.Combine(IMAGES_PATH, "profilePics", id.ToString().First().ToString(), $"{id}");
        public static string GetProfilePicturesPath(int id, byte size) => Path.Combine(GetProfilePicturesUser(id), $"{size}");
        
    }
}
