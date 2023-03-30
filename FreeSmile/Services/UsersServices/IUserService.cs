using FreeSmile.DTOs;
using static FreeSmile.Services.Helper;

namespace FreeSmile.Services
{
    public interface IUserService
    {
        public Task<RegularResponse> AddUserAsync(UserRegisterDto user);
        public Task<RegularResponse> AddPatientAsync(UserRegisterDto user, IResponseCookies cookies);
        public Task<RegularResponse> AddDentistAsync(UserRegisterDto user, IResponseCookies cookies);
        public Task<RegularResponse> AddAdminAsync(UserRegisterDto user, IResponseCookies cookies);
        public Task<RegularResponse> VerifyAccount(string otp, int user_id);
        public Task<RegularResponse> Login(UserLoginDto value, IResponseCookies cookies);
        public Task<RegularResponse> RequestEmailOtp(int user_id);
        public Task<RegularResponse> ChangePassword(ResetPasswordDto value);
        public Task<RegularResponse> ChangePassword(ChangeKnownPasswordDto value, int user_id_int);
        public Task<RegularResponse> RequestEmailOtp(string usernameOrEmail);
        public Task<RegularResponse> RedirectToHome(int user_id);

        // TODO: DELETE: delete my account

    }
}
