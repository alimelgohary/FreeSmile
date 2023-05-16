using FreeSmile.DTOs.Auth;
using FreeSmile.DTOs.Posts;
using FreeSmile.DTOs.Settings;
using static FreeSmile.Services.Helper;

namespace FreeSmile.Services
{
    public interface IDentistService
    {
        public Task<RegularResponse> AddVerificationRequestAsync(VerificationDto verificationDto, int ownerId);
        public Task<RegularResponse> DeleteVerificationRequestAsync(int user_id_int);
        public Task<GetDentistSettingsDto> GetSettingsAsync(int user_id);
        public Task<GetDentistSettingsDto> UpdateSettingsAsync(SetDentistSettingsDto settings, int user_id);
        public Task<GetDentistSettingsDto> GetPublicSettingsAsync(int auth_user_id_int, int other_user_id);
        public Task<List<GetCaseDto>> GetPatientsCasesAsync(int user_id, int size, int[] previouslyFetched, int gov_id, int case_type_id);
        public Task<int> AddSharingAsync(AddSharingDto value, int user_id);
        public Task<int> AddListingAsync(AddListingDto value, int user_id);
        public Task<int> AddArticleAsync(AddArticleDto value, int user_id);
        public Task<RegularResponse> UpdateSharingAsync(UpdateSharingDto value, int user_id);
        public Task<RegularResponse> UpdateListingAsync(UpdateListingDto value, int user_id);
        public Task<RegularResponse> UpdateArticleAsync(UpdateArticleDto value, int user_id);
        public Task<List<GetListingDto>> GetListingsAsync(int user_id, int size, int[] previouslyFetched, int governorateId, int listingCategoryId, byte sortBy);
        public Task<List<GetArticleDto>> GetArticlesAsync(int user_id, int size, int[] previouslyFetched, int articleCategoryId, byte sortBy);

        // TODO: GET: post_by_id
        // TODO: GET: article_by_id
        // TODO: GET: listing_by_id
        // TODO: GET: sharing_by_id
        // TOOD: add portfolio          //TODO: delete portfolio
        // TOOD: PUT: like/unlike article      //TODO: POST: add/remove comment on an article   //TODO: POST: Report a comment
        // TODO: Don't forget to not include blocked or blocking people posts
    }
}
