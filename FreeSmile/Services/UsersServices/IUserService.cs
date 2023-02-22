using FreeSmile.DTOs;
using FreeSmile.Models;
using static FreeSmile.Services.Helper;

namespace FreeSmile.Services
{
    public interface IUserService
    {
        public Task<RegularResponse> AddUserAsync(UserRegisterDto user, IResponseCookies cookies);
        public Task<RegularResponse> VerifyAccount(string otp, int user_id);
        public Task<RegularResponse> Login(UserLoginDto value, IResponseCookies cookies);
        public Task<RegularResponse> RequestEmailOtp(int user_id);

    }
}
