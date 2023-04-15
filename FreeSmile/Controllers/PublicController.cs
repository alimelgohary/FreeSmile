using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using FreeSmile.DTOs;
using FreeSmile.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static FreeSmile.Services.Helper;
using FreeSmile.ActionFilters;
using Swashbuckle.AspNetCore.Annotations;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Buffers.Text;

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

        [SwaggerOperation(Summary = $"Takes optional {nameof(user_id)} & {nameof(size)} (1: 100 x 100, 2: 250 x 250, 3: original size) as query & gets his profile picture as base64. If {nameof(user_id)} not specified, it returns authorized user's profile picture")]
        [HttpGet("GetProfilePicture")]
        public async Task<IActionResult> GetProfilePictureAsync(int user_id, int size)
        {
            if (user_id != 0)
            {
                byte[] picture = await _commonService.GetProfilePictureAsync(user_id, size);
                return Ok(picture);
            }

            string? authenticated_user_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(authenticated_user_id))
            {
                int authenticated_user_id_int = int.Parse(authenticated_user_id);
                byte[] picture = await _commonService.GetProfilePictureAsync(authenticated_user_id_int, size);
                return Ok(picture);
            }

            return Unauthorized();
        }
    }
}
