using FreeSmile.DTOs;
using FreeSmile.Models;

namespace FreeSmile.Services
{
    public interface IUserService
    {
        public Task<int> AddUserAsync(UserRegisterDto user);
        
    }
}
