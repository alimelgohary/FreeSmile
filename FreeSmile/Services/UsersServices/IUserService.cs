﻿using DTOs;
using FreeSmile.DTOs.Auth;
using static FreeSmile.Services.Helper;

namespace FreeSmile.Services
{
    public interface IUserService
    {
        public Task<int> AddUserAsync(UserRegisterDto user);
        public Task<RegularResponse> AddPatientAsync(UserRegisterDto user, IResponseCookies cookies);
        public Task<RegularResponse> AddDentistAsync(UserRegisterDto user, IResponseCookies cookies);
        public Task<RegularResponse> AddAdminAsync(UserRegisterDto user, IResponseCookies cookies);
        public Task<RegularResponse> VerifyAccount(string otp, int user_id, string roleString, IResponseCookies cookies);
        public Task<RegularResponse> Login(UserLoginDto value, IResponseCookies cookies);
        public Task<RegularResponse> RequestEmailOtp(int user_id, string roleString);
        public Task<RegularResponse> ChangePassword(ResetPasswordDto value);
        public Task<RegularResponse> ChangePassword(ChangeKnownPasswordDto value, int user_id_int);
        public Task<RegularResponse> RequestEmailOtp(string usernameOrEmail);
        public RegularResponse RedirectToHome(int user_id, string roleString);
        public Task<RegularResponse> DeleteMyAccount(DeleteMyAccountDto value, int user_id, IResponseCookies cookies);
        public Task<RoleWithBasicUserInfo> GetBasicUserInfo(int auth_user, int id);
    }
}
