using FreeSmile.DTOs.Admins;
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

        // TODO: PUT: admin suspend user by ID (post, comment, review)
        // TODO: PUT: admin show all reviews (From landing endpoint)
        // TODO: DELETE: admin delete offensive reviews
        // TODO: GET: superadmin show admins
        // TODO: PUT: superadmin suspend admin
        // TOOD: GET: logs for super admin
        // TOOD: PUT: edit admin settings
        // TOOD: admin add, edit, delete casetype, listingCategory, articleCat, academic degree, university
        // TODO: GET: show reports of posts, comments with (post/comment writer_id)
    }
}
