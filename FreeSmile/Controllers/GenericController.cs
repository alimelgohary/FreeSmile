using FreeSmile.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using FreeSmile.DTOs;
using FreeSmile.Services;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace FreeSmile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenericController : ControllerBase
    {
        private readonly IStringLocalizer<GenericController> _localizer;
        private readonly FreeSmileContext _context;

        public GenericController(IStringLocalizer<GenericController> localizer, FreeSmileContext context)
        {
            _localizer = localizer;
            _context = context;
        }

        [SwaggerOperation(Summary = "Returns universities list as {id, name}")]
        [HttpGet("GetUniversities")]
        public IActionResult GetUniversityList()
        {
            string lang = Thread.CurrentThread.CurrentCulture.Name;
            var universities = _context.Universities.AsEnumerable().OrderBy(x=> x.Lang(lang)).Select(u => new { id = u.UniversityId, name = u.Lang(lang) });
            return Ok(universities);
        }
        [SwaggerOperation(Summary = "returns academic degrees list as {id, name}")]
        [HttpGet("GetAcademicDegrees")]
        public IActionResult GetAcademicDegrees()
        {
            string lang = Thread.CurrentThread.CurrentCulture.Name;
            var academicDegrees = _context.AcademicDegrees.Select(x => new { id = x.DegId, name = x.Lang(lang) }).ToList();
            return Ok(academicDegrees);
        }

        [SwaggerOperation(Summary = "returns Governorates list as {id, name}")]
        [HttpGet("GetGovernorates")]
        public IActionResult GetGovernatesList()
        {
            string lang = Thread.CurrentThread.CurrentCulture.Name;
            var govs = _context.Governates.AsEnumerable().OrderBy(x => x.Lang(lang)).Select(x => new { id = x.GovId, name = x.Lang(lang) }).ToList();
            return Ok(govs);
        }

        [SwaggerOperation(Summary = "returns case types list as {id, name}")]
        [HttpGet("GetCaseTypes")]
        public IActionResult GetTypesList()
        {
            string lang = Thread.CurrentThread.CurrentCulture.Name;
            var types = _context.CaseTypes.AsEnumerable().OrderBy(x => x.Lang(lang)).Select(x => new { id = x.CaseTypeId, name = x.Lang(lang) }).ToList();
            return Ok(types);
        }

        [SwaggerOperation(Summary = "returns article categories list as {id, name}")]
        [HttpGet("GetArticleCats")]
        public IActionResult GetArticleCatsList()
        {
            string lang = Thread.CurrentThread.CurrentCulture.Name;
            var artCats = _context.ArticleCats.AsEnumerable().OrderBy(x => x.Lang(lang)).Select(x => new { id = x.ArticleCatId, name = x.Lang(lang) }).ToList();
            return Ok(artCats);
        }

        [SwaggerOperation(Summary = "returns listings (marketplace) categories list as {id, name}")]
        [HttpGet("GetListingCats")]
        public IActionResult GetProductCatsList()
        {
            string lang = Thread.CurrentThread.CurrentCulture.Name;
            var productCats = _context.ProductCats.AsEnumerable().OrderBy(x => x.Lang(lang)).Select(x => new { id = x.ProductCatId, name = x.Lang(lang) }).ToList();
            return Ok(productCats);
        }
        [SwaggerOperation(Summary = "(maybe useful for admin dashboard) returns notification templates list as {id, name, body, icon, nextPage}")]
        [HttpGet("GetNotificationTemplates")]
        public IActionResult GetNotificationTemplatesList()
        {
            string lang = Thread.CurrentThread.CurrentCulture.Name;
            var productCats = _context.NotificationTemplates.AsEnumerable().OrderBy(x => x.Lang(lang)).Select(x => new { id = x.TempId, name = x.TempName, body = x.Lang(lang), icon = x.Icon, nextPage = x.NextPage }).ToList();
            return Ok(productCats);
        }
    }
}
