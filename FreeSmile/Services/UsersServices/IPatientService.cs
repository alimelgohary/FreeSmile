﻿using FreeSmile.DTOs.Posts;
using FreeSmile.DTOs.Settings;

namespace FreeSmile.Services
{
    public interface IPatientService
    {
        public Task<GetPatientSettingsDto> GetSettingsAsync(int user_id);
        public Task<GetPatientSettingsDto> UpdateSettingsAsync(SetPatientSettingsDto settings, int user_id);
        public Task<GetPatientSettingsDto> GetPublicSettingsAsync(int auth_user_id_int, int other_user_id);
        public Task<List<GetPostDto>> GetDentistsCases(int user_id, int size, int[] previouslyFetched, int gov_id, int case_type_id);

        // TODO: GET: search results
        // TODO: Don't forget to not include blocked or blocking people posts
    }
}
