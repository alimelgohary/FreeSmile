using FreeSmile.Controllers;
using FreeSmile.DTOs;
using FreeSmile.Models;
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

        public async Task<ResponseDTO> AddUserAsync(UserRegisterDto userDto)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                // headache for checking .edu.eg
                //int gradDegId = 1; // (1 = grad)
                //if (!userDto.Email.EndsWith(".edu.eg") && userDto.DegreeRequested != gradDegId)
                //{
                //    responseDto.Id = -1;
                //    responseDto.Error = _localizer["undergradedu"];
                //    return responseDto;
                //}


                var responseDto = await _userService.AddUserAsync(userDto);

                var dentist = new Dentist()
                {
                    DentistId = responseDto.Id
                    // current university and current degree are defaulted at first
                };
                await _context.AddAsync(dentist);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return responseDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                transaction.Rollback();
                throw;
            }

        }
        public async Task<ResponseDTO> AddVerificationRequestAsync(VerificationDto verificationDto, int ownerId)
        {
            try
            {
                if (_context.VerificationRequests.Any(v => v.OwnerId == ownerId))
                    return new ResponseDTO()
                    {
                        Id = -1,
                        Error = _localizer["AlreadyRequested"]
                    };

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
                        DegreeRequested = verificationDto.DegreeRequested
                    });

                await _context.SaveChangesAsync();

                return new ResponseDTO()
                {
                    Id = -1,
                    Error = ""
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
    }
}

