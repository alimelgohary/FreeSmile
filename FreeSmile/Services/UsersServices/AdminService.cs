using FreeSmile.DTOs.Admins;
using Microsoft.Extensions.Localization;
using static FreeSmile.Services.DirectoryHelper;
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

        public AdminService(ILogger<AdminService> logger, FreeSmileContext context, IStringLocalizer<AdminService> localizer, ICommonService commonService)
        {
            _logger = logger;
            _context = context;
            _localizer = localizer;
            _commonService = commonService;
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
    }
}
