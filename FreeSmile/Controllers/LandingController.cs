﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using FreeSmile.DTOs;
using FreeSmile.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static FreeSmile.Services.Helper;
using System.Reflection.Metadata.Ecma335;
using FreeSmile.ActionFilters;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet("GetStats")]
        public IActionResult GetStats()
        {
            int numberOfDentists = _context.Dentists.Count();
            int numberOfPatients = _context.Patients.Count();
            int numberOfPosts = _context.Posts.Count();
            return Ok(new { numberOfDentists, numberOfPatients, numberOfPosts });
        }

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

        [HttpGet("GetReviews")]
        public IActionResult GetReviews()
        {
            return Ok(_context.Reviews.OrderByDescending(y => y.ReviewId).Take(10).Select(
                x => new {
                    reviewer = x.Reviewer.Username,
                    rating = x.Rating,
                    opinion = x.Opinion
                }));
        }

        
    }
}
