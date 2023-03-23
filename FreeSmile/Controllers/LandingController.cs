using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using FreeSmile.DTOs;
using FreeSmile.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static FreeSmile.Services.Helper;
using System.Reflection.Metadata.Ecma335;
using FreeSmile.ActionFilters;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace FreeSmile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LandingController : ControllerBase
    {
        private readonly IStringLocalizer<LandingController> _localizer;
        private readonly FreeSmileContext _context;
        public LandingController(IStringLocalizer<LandingController> localizer, FreeSmileContext context)
        {
            _localizer = localizer;
            _context = context;
        }
        [SwaggerOperation(Summary = "Returns numberOfPosts, numberOfDentists, numberOfPatients")]
        [HttpGet("GetStats")]
        public IActionResult GetStats()
        {
            int numberOfDentists = _context.Dentists.Count();
            int numberOfPatients = _context.Patients.Count();
            int numberOfPosts = _context.Posts.Count();
            return Ok(new { numberOfPosts, numberOfDentists, numberOfPatients });
        }
        [SwaggerOperation(Summary = "Gets { reviewsCount, averageRating (double) } For Ex: 4.33")]
        [HttpGet("GetReviewsOverview")]
        public IActionResult GetReviewsOverview()
        {
            int reviewsCount = _context.Reviews.Count();
            double averageRating = 0;
            if (reviewsCount != 0)
            {
                averageRating = _context.Reviews.Average(x => x.Rating);
            }

            return Ok(new { reviewsCount, averageRating });
        }
        [SwaggerOperation(Summary = "Returns most recent 10 reviews in this structure {reviewer (string), rating (int) (1 : 5) , opinion (string)}")]
        [HttpGet("GetTopTenReviews")]
        public IActionResult GetReviews()
        {
            return Ok(_context.Reviews.OrderByDescending(y => y.ReviewId).Take(10).Select(
                x => new {
                    reviewer = x.Reviewer.Username,
                    rating = x.Rating,
                    opinion = x.Opinion
                }));
        }

        [SwaggerOperation(Summary = "Takes a page number (GetReviews?page={id}) and returns 5 reviews in this structure {reviewer (string), rating (int) (1 : 5) , opinion (string)}")]
        [HttpGet("GetReviews")]
        public IActionResult GetReviews(int page)
        {
            return Ok(_context.Reviews.OrderByDescending(y => y.ReviewId).Skip(5 * (page - 1)).Take(5).Select(
                x => new {
                    reviewer = x.Reviewer.Username,
                    rating = x.Rating,
                    opinion = x.Opinion
                }));
        }
    }
}
