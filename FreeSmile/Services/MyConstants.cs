namespace FreeSmile.Services
{
    public class MyConstants
    {
        public static string JWT_SECRET { get; } = Helper.GetEnvVariable("Jwt_Secret", false);
        public static string PEPPER { get; } = Helper.GetEnvVariable("PEPPER", true);
        public static string FREESMILE_CONNECTION { get; } = Helper.GetEnvVariable("FreeSmileDatabase", true)

        }
}
