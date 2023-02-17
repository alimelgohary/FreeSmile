using FreeSmile.DTOs;
using static FreeSmile.Services.Helper;

namespace FreeSmile.Services
{
    public interface IDentistService : IUserService
    {
        public Task AddVerificationRequestAsync(VerificationDto verificationDto, int ownerId);
    }
}
