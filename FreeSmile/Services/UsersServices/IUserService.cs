using FreeSmile.DTOs;
using FreeSmile.Models;
using static FreeSmile.Services.Helper;

namespace FreeSmile.Services
{
    public interface IUserService
    {
        public Task<ResponseDTO> AddUserAsync(UserRegisterDto user);
        public Task<ResponseDTO> VerifyAccount(string otp, int user_id);
    }
}
