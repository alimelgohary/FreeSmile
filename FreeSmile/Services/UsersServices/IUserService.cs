using FreeSmile.DTOs;
using FreeSmile.Models;
using Microsoft.EntityFrameworkCore;
using static FreeSmile.Services.Helper;
using static System.Net.WebRequestMethods;

namespace FreeSmile.Services
{
    public interface IUserService
    {
        public Task<RegularResponse> AddUserAsync(UserRegisterDto user, IResponseCookies cookies);
        public Task<RegularResponse> VerifyAccount(string otp, int user_id);
        public Task<RegularResponse> Login(UserLoginDto value, IResponseCookies cookies);
        public Task<RegularResponse> RequestEmailOtp(int user_id);
        public Task<RegularResponse> ChangePassword(ChangeUnknownPasswordDto value);
        public Task<RegularResponse> ChangePassword(ChangeKnownPasswordDto value, int user_id_int);
        public Task<RegularResponse> RequestEmailOtp(string usernameOrEmail);
        public Task<RegularResponse> RedirectToHome(int user_id);
    }
}
