using FreeSmile.DTOs.Admins;
using Microsoft.Extensions.Localization;
using static FreeSmile.Services.DirectoryHelper;
using static FreeSmile.Services.Helper;
using static FreeSmile.Services.EmailService;
using Microsoft.EntityFrameworkCore;
using FreeSmile.ActionFilters;

namespace FreeSmile.Services
{
    public class AdminService : IAdminService
    {
        private readonly ILogger<AdminService> _logger;
        private readonly IStringLocalizer<AdminService> _localizer;
        private readonly FreeSmileContext _context;
        private readonly ICommonService _commonService;
        private readonly IDentistService _dentistService;

        public AdminService(ILogger<AdminService> logger, FreeSmileContext context, IStringLocalizer<AdminService> localizer, ICommonService commonService, IDentistService dentistService)
        {
            _logger = logger;
            _context = context;
            _localizer = localizer;
            _commonService = commonService;
            _dentistService = dentistService;
        }

        public async Task<int> GetNumberOfVerificationRequestsAsync()
        {
            return await _context.VerificationRequests.CountAsync();
        }

        public async Task<List<byte[]>> GetVerificationImagesAsync(int dentist_id)
        {
            var path = GetVerificationPathUser(dentist_id);
            List<byte[]> imageList = new List<byte[]>();
            await Parallel.ForEachAsync(Directory.EnumerateFiles(path), async (file, token) =>
            {
                byte[] bytes = await File.ReadAllBytesAsync(file);
                lock (imageList)
                {
                    imageList.Add(bytes);
                }
            });
            return imageList;
        }

        public async Task<GetVerificationRequestDto> GetVerificationRequestAsync(int number)
        {
            var request = await _context.VerificationRequests.AsNoTracking()
                .Select(x => new GetVerificationRequestDto
                {
                    RequestedUniversity = x.UniversityRequestedNavigation.Lang(Thread.CurrentThread.CurrentCulture.Name),
                    RequestedDegree = x.DegreeRequestedNavigation.Lang(Thread.CurrentThread.CurrentCulture.Name),
                    UserId = x.OwnerId,
                    FullName = x.Owner.DentistNavigation.Fullname,
                    Username = x.Owner.DentistNavigation.Username
                })
                .OrderBy(x => x.UserId)
                .Skip(number - 1)
                .FirstOrDefaultAsync();

            if (request is not null)
            {
                if (await GetNumberOfVerificationRequestsAsync() == number)
                    request.IsLast = true;
                request.Images = await GetVerificationImagesAsync(request.UserId);
                request.ProfilePicture = await _commonService.GetProfilePictureDangerousAsync(request.UserId);
                return request;
            }
            throw new NotFoundException(_localizer["NotFound", _localizer["request"]]);
        }

        public async Task<RegularResponse> AcceptVerificationRequestAsync(int dentist_id)
        {
            //TODO: Make this a transaction?
            var v = await _context.VerificationRequests.AsNoTracking()
                                  .Select(x => new { x.OwnerId, x.UniversityRequested, x.DegreeRequested })
                                  .FirstOrDefaultAsync(x => x.OwnerId == dentist_id);
            
            var dentist = await _context.Dentists.FindAsync(dentist_id)!;
            dentist!.IsVerifiedDentist = true;
            dentist.CurrentDegree = v!.DegreeRequested;
            dentist.CurrentUniversity = v.UniversityRequested;
            await _context.SaveChangesAsync();
            await _dentistService.DeleteVerificationRequestAsync(dentist_id);
            
            try
            {
                string lang = Thread.CurrentThread.CurrentCulture.Name; // TODO: Should use dentist lang not admin lang
                var userInfo = await _context.Users.AsNoTracking()
                                               .Select(x => new { x.Id, x.Username, x.Email })
                                               .FirstOrDefaultAsync(u => u.Id == dentist_id);
                
                SendEmail(mailTo: userInfo!.Email,
                          subject: _localizer["RequestAcceptEmailSubject"],
                          htmlFileName: MyConstants.acceptemailfilename,
                          lang: lang,
                          gmailApi: true,
                          userInfo.Username, lang);
                
                await _commonService.AddNotificationDangerousAsync(dentist_id, NotificationTemplates.Verification_Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return RegularResponse.Success(message: _localizer["requestaccepted"]);
        }

        public async Task<RegularResponse> RejectVerificationRequestAsync(int dentist_id, int reject_reason)
        {
            await _dentistService.DeleteVerificationRequestAsync(dentist_id);

            try
            {
                string lang = Thread.CurrentThread.CurrentCulture.Name; // TODO: Should use dentist lang not admin lang

                var userInfo = await _context.Users.AsNoTracking()
                                                   .Select(x => new { x.Id, x.Username, x.Email })
                                                   .FirstOrDefaultAsync(u => u.Id == dentist_id);
                
                var temp = await _context.NotificationTemplates.AsNoTracking()
                                                                .Select(x => new { x.TempId, body = x.Lang(lang) })
                                                                .FirstOrDefaultAsync(x => x.TempId == reject_reason);
                SendEmail(mailTo: userInfo!.Email,
                          subject: _localizer["RequestRejectEmailSubject"],
                          htmlFileName: MyConstants.rejectemailfilename,
                          lang: lang,
                          gmailApi: true,
                          userInfo.Username, temp!.body, lang);

                await _commonService.AddNotificationDangerousAsync(dentist_id, (NotificationTemplates)reject_reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return RegularResponse.Success(message: _localizer["requestrejected"]);
        }
    }
}
