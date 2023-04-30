using DTOs;
﻿using FreeSmile.DTOs;
using static FreeSmile.Services.Helper;

namespace FreeSmile.Services
{
    public interface IDentistService
    {
        public Task<RegularResponse> AddVerificationRequestAsync(VerificationDto verificationDto, int ownerId);
        public Task<GetDentistSettingsDto> GetSettingsAsync(int user_id);
        public Task<GetDentistSettingsDto> UpdateSettingsAsync(SetDentistSettingsDto settings, int user_id);

        // TODO: GET Sharing patient Posts for Dentists
        // TODO: GET Patients cases Posts for Dentists
        // TODO: GET: post_by_id
        // TODO: GET: article_by_id
        // TODO: GET: listing_by_id
        // TODO: GET: sharing_by_id
        // TODO: create case for dentist          // TODO: edit case for dentist    // TODO: delete case for dentist
        // TODO: create sharing for dentist       // TODO: edit sharing for dentist // TODO: delete sharing for dentist
        // TODO: create article                   // TODO: edit article             // TODO: delete article
        // TODO: POST: create listing             // TODO: PUT: edit listing        // TODO: DELETE: listing
        // TODO: Don't forget to not include blocked or blocking people posts
        // TOOD: add portfolio          //TODO: delete portfolio
        // TOOD: PUT: like/unlike article      //TODO: POST: add/remove comment on an article   //TODO: POST: Report a comment
        // TODO: PUT: change settings dentist
        // TODO: GET: get settings dentist
        // TODO: Don't forget to not include blocked or blocking people posts
    }
}
