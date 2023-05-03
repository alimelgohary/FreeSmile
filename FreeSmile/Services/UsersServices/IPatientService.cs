using FreeSmile.DTOs;

namespace FreeSmile.Services
{
    public interface IPatientService
    {
        public Task<GetPatientSettingsDto> GetSettingsAsync(int user_id);
        public Task<GetPatientSettingsDto> UpdateSettingsAsync(SetPatientSettingsDto settings, int user_id);
        public Task<GetPatientSettingsDto> GetPublicSettingsAsync(int auth_user_id_int, int other_user_id);

        // TODO: GET: Dentists Posts for Patients
        // TODO: GET: post_by_id
        // TODO: GET: search results
        // TODO: Don't forget to not include blocked or blocking people posts
    }
}
