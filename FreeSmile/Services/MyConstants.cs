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

        //"Images\profilePics\{id}\{size}{ext}"
        public static string GetProfilePicturesUser(int id) => Path.Combine(IMAGES_PATH, "profilePics", $"{id}");
        public static string GetProfilePicturesRelativePath(int id, byte size, string ext) => Path.Combine(GetProfilePicturesUser(id), $"{size}{ext}");
        public static string GetProfilePicturesFullPath(int id, byte size, string ext) => Path.Combine(Directory.GetCurrentDirectory(), GetProfilePicturesRelativePath(id, size, ext));
    }
}
