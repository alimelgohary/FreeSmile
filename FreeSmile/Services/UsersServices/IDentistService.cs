using FreeSmile.DTOs;
using static FreeSmile.Services.Helper;

namespace FreeSmile.Services
{
    public interface IDentistService : IUserService
    {
        public Task<RegularResponse> AddVerificationRequestAsync(VerificationDto verificationDto, int ownerId);
        public Task<bool> IsVerifiedDentist(int id);
        public Task<bool> IsVerifiedDentist(string usernameOrEmail);
    }
}
