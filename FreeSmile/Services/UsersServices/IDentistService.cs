using FreeSmile.DTOs;
using static FreeSmile.Services.Helper;

namespace FreeSmile.Services
{
    public interface IDentistService
    {
        public Task<RegularResponse> AddVerificationRequestAsync(VerificationDto verificationDto, int ownerId);
        
        // TODO: GET Sharing paitents Posts for Dentists
        // TODO: GET Patients cases Posts for Dentists
        // TODO: create case for dentist          // TODO: edit case for dentist      // TODO: delete case for dentist
        // TODO: create sharing for dentist       // TODO: edit sharing for dentist   // TODO: delete sharing for dentist
        // TODO: create article                   // TODO: edit article               // TODO: delete article
        // TODO: Don't forget to not include blocked or blocking people posts
        // TOOD: add portfolio      //TODO: edit portfolio
        // TOOD: PUT: like article      //TODO: POST: comment on an article   //TODO: POST: Report a comment
        // TODO: PUT: change settings dentist
    }
}
