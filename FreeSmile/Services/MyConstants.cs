namespace FreeSmile.Services
{
    public partial class Helper
    {
        public class MyConstants
        {
            public static string JWT_SECRET { get; } = GetEnvVariable("Jwt_Secret", false);
            public static string PEPPER { get; } = GetEnvVariable("PEPPER", true);
            public static string FREESMILE_CONNECTION { get; } = GetEnvVariable("FreeSmileDatabase", true)

        }

    }
}
