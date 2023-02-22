using FreeSmile.Controllers;
using FreeSmile.DTOs;
using FreeSmile.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using static FreeSmile.Services.AuthHelper;
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

        public async Task<RegularResponse> AddUserAsync(UserRegisterDto userDto, IResponseCookies cookies)
        {
            RegularResponse response;
            
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                response = await _userService.AddUserAsync(userDto, cookies);

                var dentist = new Dentist()
                {
                    DentistId = response.Id
                    // current university and current degree are defaulted at first
                };
                await _context.AddAsync(dentist);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var tokenAge = MyConstants.REGISTER_TOKEN_AGE;
                string token = GetToken(dentist.DentistId, tokenAge, Role.Dentist);
                cookies.Append(MyConstants.AUTH_COOKIE_KEY, token, new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.Strict, MaxAge = tokenAge });

                response = new()
                {
                    Id = dentist.DentistId,
                    StatusCode = StatusCodes.Status200OK,
                    Token = token,
                    Message = _localizer["RegisterSuccess"],
                    NextPage = Pages.verifyEmail.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                transaction.Rollback();
                response = new()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Error = _localizer["UnknownError"],
                    NextPage = Pages.registerDentist.ToString()
                };
            }
            return response;

        }
        public async Task<RegularResponse> AddVerificationRequestAsync(VerificationDto verificationDto, int ownerId)
        {
            try
            {
                if (await _context.VerificationRequests.AnyAsync(v => v.OwnerId == ownerId))
                    return new RegularResponse()
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Error = _localizer["AlreadyRequested"],
                        NextPage = Pages.pendingVerificationAcceptance.ToString()
                    };

                User? user = await _context.Users.FindAsync(ownerId);

                if (user is null)
                    return new RegularResponse()
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Error = _localizer["UserNotFound"],
                        NextPage = Pages.registerDentist.ToString()
                    };
                
                if (user.IsVerified != true)
                    return new RegularResponse()
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Error = _localizer["VerifyEmailFirst"],
                        NextPage = Pages.verifyEmail.ToString()
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

                return new RegularResponse()
                {
                    Id = user.Id,
                    StatusCode = StatusCodes.Status200OK,
                    Message = _localizer["verificationrequestsuccess"],
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return new RegularResponse()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Error = _localizer["UnknownError"]
                };
            }
        }

        public Task<RegularResponse> VerifyAccount(string otp, int user_id)
        {
            return _userService.VerifyAccount(otp, user_id);
        }

        public async Task<RegularResponse> Login(UserLoginDto value, IResponseCookies cookies)
        {
            return await _userService.Login(value, cookies);
        }
        
        public async Task<RegularResponse> RequestEmailOtp(int user_id)
        {
            return await _userService.RequestEmailOtp(user_id);
        }
    }
}

