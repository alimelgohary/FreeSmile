using FreeSmile.DTOs;
using FreeSmile.Models;
using static FreeSmile.Services.Helper;

namespace FreeSmile.Services
{
    public interface IUserService
    {
        public Task<ServiceReturnType> AddUserAsync(UserRegisterDto user);
        
    }
}
