﻿using FreeSmile.DTOs;
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
        public Task<List<GetPostDto>> GetPatientsCasesAsync(int user_id, int size, int[] previouslyFetched, int gov_id, int case_type_id);
        public Task<int> AddSharingAsync(AddSharingDto value, int user_id);
        public Task<int> AddListingAsync(AddListingDto value, int user_id);
        public Task<int> AddArticleAsync(AddArticleDto value, int user_id);
        public Task<RegularResponse> UpdateSharingAsync(UpdateSharingDto value, int user_id);
        public Task<RegularResponse> UpdateListingAsync(UpdateListingDto value, int user_id);
        public Task<RegularResponse> UpdateArticleAsync(UpdateArticleDto value, int user_id);
        public Task<List<GetPostDto>> GetListingsAsync(int user_id, int size, int[] previouslyFetched, int governorateId, int listingCategoryId, byte sortBy);
        public Task<List<GetPostDto>> GetArticlesAsync(int user_id, int size, int[] previouslyFetched, int articleCategoryId, byte sortBy);
        public Task LikeUnlikeArticleAsync(int user_id_int, int articleId);
        public Task<RegularResponse> ArticleAddCommentAsync(int user_id, AddCommentDto value);
        public Task<RegularResponse> ArticleRemoveCommentAsync(int user_id, int comment_id, string roleString);

        // TOOD: POST: add portfolio
        // TODO: DELETE: remove portfolio
        // TODO: POST: Report a comment
    }
}
