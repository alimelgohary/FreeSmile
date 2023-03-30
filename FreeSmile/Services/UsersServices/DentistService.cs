using FreeSmile.Controllers;
using FreeSmile.DTOs;
using FreeSmile.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using static FreeSmile.Services.Helper;

namespace FreeSmile.Services
{
    public class DentistService : IDentistService
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IStringLocalizer<UsersController> _localizer;
        private readonly FreeSmileContext _context;
        private readonly IUserService _userService;

        public DentistService(ILogger<UsersController> logger, FreeSmileContext context, IStringLocalizer<UsersController> localizer, IUserService userService)
        {
            _logger = logger;
            _context = context;
            _localizer = localizer;
            _userService = userService;
        }

        public async Task<RegularResponse> AddVerificationRequestAsync(VerificationDto verificationDto, int ownerId)
        {
            try
            {
                if (await _context.VerificationRequests.AnyAsync(v => v.OwnerId == ownerId))
                    return RegularResponse.BadRequestError(id: ownerId, error: _localizer["AlreadyRequested"]);

                User? user = await _context.Users.FindAsync(ownerId)!;

                var natExt = Path.GetExtension(verificationDto.NatIdPhoto.FileName);
                var natRelativePath = Path.Combine("Images", "verificationRequests", $"{ownerId}nat{natExt}");
                var natFullPath = Path.Combine(Directory.GetCurrentDirectory(), natRelativePath);
                await SaveToDisk(verificationDto.NatIdPhoto, natFullPath);

                var proofExt = Path.GetExtension(verificationDto.ProofOfDegreePhoto.FileName);
                var proofRelativePath = Path.Combine("Images", "verificationRequests", $"{ownerId}proof{proofExt}");
                var proofFullPath = Path.Combine(Directory.GetCurrentDirectory(), proofRelativePath);
                await SaveToDisk(verificationDto.ProofOfDegreePhoto, proofFullPath);


                await _context.AddAsync(
                    new VerificationRequest()
                    {
                        OwnerId = ownerId,
                        NatIdPhoto = natRelativePath,
                        ProofOfDegreePhoto = proofRelativePath,
                        DegreeRequested = (int)verificationDto.DegreeRequested!,
                        UniversityRequested = (int)verificationDto.UniversityRequested!
                    });

                await _context.SaveChangesAsync();

                return RegularResponse.Success(id: user.Id, message: _localizer["verificationrequestsuccess"], nextPage: Pages.pendingVerificationAcceptance.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError("{Message}", ex.Message);

                return RegularResponse.UnknownError(_localizer);
            }
        }
    }
}

