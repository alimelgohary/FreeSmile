using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using FreeSmile.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static FreeSmile.Services.Helper;
using FreeSmile.ActionFilters;
using Swashbuckle.AspNetCore.Annotations;
using FreeSmile.DTOs.Settings;
using FreeSmile.DTOs.Auth;
using FreeSmile.DTOs.Posts;
using FreeSmile.CustomValidations;
using System.ComponentModel.DataAnnotations;
using FreeSmile.DTOs.Query;

namespace FreeSmile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(NotSuspended), Order = 1)]
    [ServiceFilter(typeof(VerifiedEmailTurbo), Order = 2)]
    [ServiceFilter(typeof(VerifiedIfDentistTurbo), Order = 3)]
    [Authorize(Roles = "Dentist")]
    public class DentistsController : ControllerBase
    {
        private readonly IStringLocalizer<DentistsController> _localizer;
        private readonly IDentistService _dentistService;

        public DentistsController(IStringLocalizer<DentistsController> localizer, IDentistService dentistService)
        {
            _localizer = localizer;
            _dentistService = dentistService;
        }


        [SwaggerOperation(Summary = $"Gets Dentist profile settings in the form of {nameof(GetDentistSettingsDto)}")]
        [HttpGet("GetSettings")]
        public async Task<IActionResult> GetSettingsAsync()
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            var res = await _dentistService.GetSettingsAsync(user_id_int);

            return Ok(res);
        }


        [SwaggerOperation(Summary = $"Takes a {nameof(SetDentistSettingsDto)} as JSON and updates Dentist profile settings. Send new values of each field, DO NOT SEND USERNAME IF IT'S NOT UPDATED. It returns updated settings as {nameof(GetDentistSettingsDto)}")]
        [HttpPut("UpdateSettings")]
        public async Task<IActionResult> UpdateSettingsAsync([FromBody] SetDentistSettingsDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            var res = await _dentistService.UpdateSettingsAsync(value, user_id_int);

            return Ok(res);
        }


        [SwaggerOperation(Summary = $"Takes {nameof(pageSize.Size)}, {nameof(previouslyFetched)} as integer array (id1,id2,id3), {nameof(gov_id.GovernorateId)}, {nameof(caseTypeDto.CaseTypeId)} as query and returns patients cases in the requested governorate, and caseType. DEFAULT is dentist's university location and all types. (returns List of {nameof(GetPostDto)})")]
        [HttpGet("GetPatientsCases")]
        public async Task<IActionResult> GetPatientsCasesAsync([FromQuery] SizeDto pageSize, [FromQuery][CommaArrayInt()][Display(Name = nameof(previouslyFetched))] string? previouslyFetched, [FromQuery] QueryGovernorate gov_id, [FromQuery] QueryCaseType caseTypeDto)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            int[] ids = previouslyFetched?.Split(',').Select(x => int.Parse(x)).ToArray() ?? Array.Empty<int>();
            List<GetPostDto> result = await _dentistService.GetPatientsCasesAsync(user_id_int, pageSize.Size, ids, gov_id.GovernorateId, caseTypeDto.CaseTypeId);

            return Ok(result);
        }


        [SwaggerOperation(Summary = $"Takes {nameof(pageSize.Size)}, {nameof(previouslyFetched)} as integer array (id1,id2,id3), {nameof(gov_id.GovernorateId)}, {nameof(productCatDto.ListingCategoryId)}, {nameof(sortBy)} 0 : Date Desc, 1 : Date Asc, 2 : Price Asc, 3 : Price Desc as query and returns products listed in the requested governorate, and product category. DEFAULT is dentist's university location and all cats. (returns List of {nameof(GetPostDto)})")]
        [HttpGet("GetListings")]
        public async Task<IActionResult> GetListingsAsync([FromQuery] SizeDto pageSize, [FromQuery][CommaArrayInt()][Display(Name = nameof(previouslyFetched))] string? previouslyFetched, [FromQuery] QueryGovernorate gov_id, [FromQuery] QueryProductCat productCatDto, [FromQuery] byte sortBy)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            int[] ids = previouslyFetched?.Split(',').Select(x => int.Parse(x)).ToArray() ?? Array.Empty<int>();
            List<GetPostDto> result = await _dentistService.GetListingsAsync(user_id_int, pageSize.Size, ids, gov_id.GovernorateId, productCatDto.ListingCategoryId, sortBy);

            return Ok(result);
        }


        [SwaggerOperation(Summary = $"Takes {nameof(pageSize.Size)}, {nameof(previouslyFetched)} as integer array (id1,id2,id3), {nameof(queryArticleCat.ArticleCategoryId)}, {nameof(sortBy)} 0 : Date Desc, 1 : Date Asc, 2 : Likes Desc, 3 : Comments Desc as query and returns articles in the requested category. DEFAULT is all cats. (returns List of {nameof(GetPostDto)})")]
        [HttpGet("GetArticles")]
        public async Task<IActionResult> GetArticlesAsync([FromQuery] SizeDto pageSize, [FromQuery][CommaArrayInt()][Display(Name = nameof(previouslyFetched))] string? previouslyFetched, [FromQuery] QueryArticleCat queryArticleCat, [FromQuery] byte sortBy)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            int[] ids = previouslyFetched?.Split(',').Select(x => int.Parse(x)).ToArray() ?? Array.Empty<int>();
            List<GetPostDto> result = await _dentistService.GetArticlesAsync(user_id_int, pageSize.Size, ids, queryArticleCat.ArticleCategoryId, sortBy);

            return Ok(result);
        }


        [SwaggerOperation(Summary = $"Takes {nameof(AddSharingDto)} as Form Data & creates a sharing patient post. GovernorateId is optional. Default is Dentist's current university location. This should return integer created post id if Success")]
        [HttpPost("AddSharing")]
        public async Task<IActionResult> AddSharingAsync([FromForm] AddSharingDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);
            int res = await _dentistService.AddSharingAsync(value, user_id_int);

            return Ok(res);
        }


        [SwaggerOperation(Summary = $"Takes {nameof(UpdateSharingDto)} as JSON & updates a sharing patient post. This should return {nameof(RegularResponse)} with a success message")]
        [HttpPut("UpdateSharing")]
        public async Task<IActionResult> UpdateSharingAsync([FromBody] UpdateSharingDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _dentistService.UpdateSharingAsync(value, user_id_int);

            return StatusCode(res.StatusCode, res);
        }


        [SwaggerOperation(Summary = $"Takes {nameof(AddListingDto)} as Form Data & creates a listing post (product). GovernorateId is optional. Default is Dentist's current university location. This should return integer created post id if Success")]
        [HttpPost("AddListing")]
        public async Task<IActionResult> AddListingAsync([FromForm] AddListingDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);
            int res = await _dentistService.AddListingAsync(value, user_id_int);

            return Ok(res);
        }


        [SwaggerOperation(Summary = $"Takes {nameof(UpdateListingDto)} as JSON & updates a listing. This should return {nameof(RegularResponse)} with a success message")]
        [HttpPut("UpdateListing")]
        public async Task<IActionResult> UpdateListingAsync([FromBody] UpdateListingDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _dentistService.UpdateListingAsync(value, user_id_int);

            return StatusCode(res.StatusCode, res);
        }


        [SwaggerOperation(Summary = $"Takes {nameof(AddArticleDto)} as Form Data & creates an article. This should return integer created post id if Success")]
        [HttpPost("AddArticle")]
        public async Task<IActionResult> AddArticleAsync([FromForm] AddArticleDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);
            int res = await _dentistService.AddArticleAsync(value, user_id_int);

            return Ok(res);
        }


        [SwaggerOperation(Summary = $"Takes {nameof(UpdateArticleDto)} as JSON & updates article. This should return {nameof(RegularResponse)} with a success message")]
        [HttpPut("UpdateArticle")]
        public async Task<IActionResult> UpdateArticleAsync([FromBody] UpdateArticleDto value)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            RegularResponse res = await _dentistService.UpdateArticleAsync(value, user_id_int);

            return StatusCode(res.StatusCode, res);
        }

        [SwaggerOperation(Summary = $"Takes {nameof(articleId)} as query and Likes or unlikes that certain article")]
        [HttpPut("LikeUnlikeArticle")]
        public async Task<IActionResult> LikeUnlikeArticleAsync([FromQuery] int articleId)
        {
            string user_id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;
            int user_id_int = int.Parse(user_id);

            await _dentistService.LikeUnlikeArticleAsync(user_id_int, articleId);

            return Ok();
        }


        #region DummyActions
        [HttpPost("Dummy")]
        public void DummyAction2(VerificationDto v) { } // Only for including VerificationDto in Swagger
        [HttpPost("Dummy2")]
        public void DummyAction3(RegularResponse r) { } // Only for including General api response in Swagger
        [HttpPost("Dummy3")]
        public void DummyAction4(GetDentistSettingsDto r) { } // Only for including GetDentistSettingsDto in Swagger
        #endregion

    }
}
