﻿using FreeSmile.ActionFilters;
using FreeSmile.Controllers;
using FreeSmile.DTOs;
using FreeSmile.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using static FreeSmile.Services.Helper;
using static FreeSmile.Services.DirectoryHelper;
using DTOs;

namespace FreeSmile.Services
{
    public class DentistService : IDentistService
    {
        private readonly ILogger<DentistService> _logger;
        private readonly IStringLocalizer<DentistService> _localizer;
        private readonly FreeSmileContext _context;
        private readonly IUserService _userService;
        private readonly ICommonService _commonService;

        public DentistService(ILogger<DentistService> logger, FreeSmileContext context, IStringLocalizer<DentistService> localizer, IUserService userService, ICommonService commonService)
        {
            _logger = logger;
            _context = context;
            _localizer = localizer;
            _userService = userService;
            _commonService = commonService;
        }

        public async Task<RegularResponse> AddVerificationRequestAsync(VerificationDto verificationDto, int ownerId)
        {
            if (await _context.VerificationRequests.AnyAsync(v => v.OwnerId == ownerId))
                throw new GeneralException(_localizer["AlreadyRequested"]);

            var userDir = GetVerificationPathUser(ownerId);
            if (!Directory.Exists(userDir))
            {
                Directory.CreateDirectory(userDir);
            }

            var natPath = GetVerificationImgPath(ownerId, VerificationType.Nat);
            var proofPath = GetVerificationImgPath(ownerId, VerificationType.Proof);

            try
            {
                byte[] natImage;
                using (var memoryStream = new MemoryStream())
                {
                    await verificationDto.NatIdPhoto.CopyToAsync(memoryStream);
                    natImage = memoryStream.ToArray();
                }
                var natExt = Path.GetExtension(verificationDto.NatIdPhoto.FileName);
                var encoder = ExtensionToEncoder(natExt);
                using (var image = Image.Load(natImage))
                {
                    await image.SaveAsync(natPath, encoder);
                }

                byte[] proofImage;
                using (var memoryStream = new MemoryStream())
                {
                    await verificationDto.ProofOfDegreePhoto.CopyToAsync(memoryStream);
                    proofImage = memoryStream.ToArray();
                }
                var proofExt = Path.GetExtension(verificationDto.ProofOfDegreePhoto.FileName);
                encoder = ExtensionToEncoder(proofExt);
                using (var image = Image.Load(natImage))
                {
                    await image.SaveAsync(proofPath, encoder);
                }

                await _context.AddAsync(
                    new VerificationRequest()
                    {
                        OwnerId = ownerId,
                        DegreeRequested = (int)verificationDto.DegreeRequested!,
                        UniversityRequested = (int)verificationDto.UniversityRequested!
                    });

                await _context.SaveChangesAsync();

                return RegularResponse.Success(message: _localizer["VerificationRequestSuccess"], nextPage: Pages.pendingVerificationAcceptance.ToString());

            }
            
            catch (Exception ex) when (ex is UnknownImageFormatException
                                    || ex is InvalidImageContentException
                                    || ex is NotSupportedException)
            {
                if (Directory.Exists(userDir))
                    Directory.Delete(userDir, true);

                throw new GeneralException(_localizer["ImagesOnly", _localizer["SelectedPic"]]);
            }
            catch (Exception)
            {
                if (Directory.Exists(userDir))
                    Directory.Delete(userDir, true);

                throw;
            }
        }

        public async Task<GetDentistSettingsDto> GetSettingsAsync(int user_id)
        {
            var lang = Thread.CurrentThread.CurrentCulture.Name;
            GetCommonSettingsDto dto = await _commonService.GetCommonSettingsAsync(user_id);
            var dentist = await _context.Dentists.AsNoTracking()
                                            .Select(x => new GetDentistSettingsDto()
                                            {
                                                UserId = dto.UserId,
                                                FullName = dto.FullName,
                                                Username = dto.Username,
                                                Phone = dto.Phone,
                                                Email = dto.Email,
                                                Birthdate = dto.Birthdate,
                                                VisibleMail = dto.VisibleMail,
                                                VisibleContact = dto.VisibleContact,
                                                ProfilePicture = dto.ProfilePicture,
                                                AcademicDegree = x.CurrentDegreeNavigation.Lang(lang),
                                                University = x.CurrentUniversityNavigation.Lang(lang),
                                                Bio = x.Bio,
                                                FbUsername = x.FbUsername,
                                                LinkedUsername = x.LinkedUsername,
                                                GScholarUsername = x.GScholarUsername,
                                                ResearchGateUsername = x.ResearchGateUsername,
                                            })
                                            .FirstOrDefaultAsync(x => x.UserId == user_id);
            dentist!.HasPendingVerificationRequest = await _context.VerificationRequests.AnyAsync(x => x.OwnerId == user_id);
            return dentist;
        }

        public Task<GetDentistSettingsDto> UpdateSettingsAsync(SetDentistSettingsDto settings, int user_id)
        {
            throw new NotImplementedException();
        }
    }
}

