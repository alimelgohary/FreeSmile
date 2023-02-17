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
            ResponseDTO responseDto;
            DentistRegisterDto? dentistDto = userDto as DentistRegisterDto;

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                int gradDegId = 1; // (1 = grad)
                if (!dentistDto.Email.EndsWith(".edu.eg") && dentistDto.DegreeRequested != gradDegId)
                {
                    responseDto.Id = -1;
                    responseDto.Error = _localizer["undergradedu"];
                    return responseDto;
                }


                responseDto = await _userService.AddUserAsync(userDto);

                var dentist = new Dentist()
                {
                    DentistId = responseDto.Id,
                    CurrentUniversity = dentistDto!.CurrentUniversity,
                };
                await _context.AddAsync(dentist);
                await _context.SaveChangesAsync();

                //What if fake EXt
                var natExt = Path.GetExtension(dentistDto.NatIdPhoto.FileName);
                var natRelativePath = Path.Combine("Images", "verificationRequests", $"{responseDto.Id}nat{natExt}");
                var natFullPath = Path.Combine(Directory.GetCurrentDirectory(), natRelativePath);
                await SaveToDisk(dentistDto.NatIdPhoto, natFullPath);

                var proofFile = dentistDto.ProofOfDegreePhoto;
                var proofExt = Path.GetExtension(proofFile?.FileName);
                var proofRelativePath = Path.Combine("Images", "verificationRequests", $"{responseDto.Id}proof{proofExt}");
                var proofFullPath = Path.Combine(Directory.GetCurrentDirectory(), proofRelativePath);
                await SaveToDisk(dentistDto.ProofOfDegreePhoto, proofFullPath);


                await _context.AddAsync(
                    new VerificationRequest()
                    {
                        OwnerId = responseDto.Id,
                        NatIdPhoto = natRelativePath,
                        ProofOfDegreePhoto = proofFile is null ? null : proofRelativePath,
                        DegreeRequested = dentistDto.DegreeRequested
                    });

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
    }
}

