using FreeSmile.DTOs.Admins;
using FreeSmile.DTOs.Posts;
using static FreeSmile.Services.Helper;

namespace FreeSmile.Services
{
    public interface IAdminService
    {
        public Task<int> GetNumberOfVerificationRequestsAsync();
        public Task<GetVerificationRequestDto> GetVerificationRequestAsync(int number);
        public Task<List<byte[]>> GetVerificationImagesAsync(int dentist_id);
        public Task<RegularResponse> AcceptVerificationRequestAsync(int dentist_id);
        public Task<RegularResponse> RejectVerificationRequestAsync(int dentist_id, int reject_reason);
        public Task<List<GetPostDto>> GetAllPosts(int page, int size);
        public Task<List<LogDto>> GetLogsAsync(string? dt);
        public Task<List<LogSummaryDto>> GetLogsSummaryAsync(string? dt);

        // TODO: PUT: admin suspend user by ID (post, comment, review)
        // TODO: PUT: admin show all reviews (From landing endpoint)
        // TODO: DELETE: admin delete offensive reviews
        // TODO: GET: superadmin show admins
        // TODO: PUT: superadmin suspend admin
        // TOOD: PUT: edit admin settings
        // TOOD: admin add, edit, delete casetype, listingCategory, articleCat, academic degree, university
        // TODO: GET: show reports of posts, comments with (post/comment writer_id)
        // TODO: GET: List of suspended users
        // TODO: PUT: Unsuspend user
    }
}
