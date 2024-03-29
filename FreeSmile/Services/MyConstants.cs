﻿namespace FreeSmile.Services
{
    public class MyConstants
    {
        public static string FREESMILE_CONNECTION { get; } = Helper.GetEnvVariable("_FreeSmileDatabase");

        public static TimeSpan REGISTER_TOKEN_AGE = TimeSpan.FromHours(1);
        public static TimeSpan LOGIN_TOKEN_AGE = TimeSpan.FromDays(1);
        public const string AUTH_COOKIE_KEY = "Authorization-Token";
        public static TimeSpan OTP_AGE = TimeSpan.FromMinutes(10);
        public const string otpemailfilename = "otpemail.html";
        public const string rejectemailfilename = "requestreject.html";
        public const string acceptemailfilename = "requestaccept.html";
        public const long MAX_IMAGE_SIZE = 153600L; // 150KB
    }
}
