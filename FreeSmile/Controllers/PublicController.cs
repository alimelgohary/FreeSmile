using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using FreeSmile.Services;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;

namespace FreeSmile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicController : ControllerBase
    {
        private readonly IStringLocalizer<PublicController> _localizer;
        private readonly IDentistService _dentistService;
        private readonly IPatientService _patientService;
        private readonly ICommonService _commonService;

        public PublicController(IStringLocalizer<PublicController> localizer, IDentistService dentistService, IPatientService patientService, ICommonService commonService)
        {
            _localizer = localizer;
            _dentistService = dentistService;
            _patientService = patientService;
            _commonService = commonService;
        }

        [SwaggerOperation(Summary = $"Takes optional {nameof(other_user_id)} & {nameof(size)} (1: 100, 2: 250, 3: original size) as query & gets his profile picture as base64. If {nameof(other_user_id)} not specified, it returns authorized user's profile picture. If {nameof(size)} not specified, it returns size 1")]
        [HttpGet("GetProfilePicture")]
        public async Task<IActionResult> GetProfilePictureAsync(uint other_user_id, byte size)
        {
            string? auth_user_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int auth_user_id_int = 0;
            if (!string.IsNullOrEmpty(auth_user_id))
                auth_user_id_int = int.Parse(auth_user_id);

            byte[]? picture = await _commonService.GetProfilePictureAsync(auth_user_id_int, (int)other_user_id, size);
            return Ok(picture);
        }
    }
}
