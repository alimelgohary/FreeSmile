using FreeSmile.Controllers;
using FreeSmile.DTOs;
using FreeSmile.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using System.Transactions;
using static FreeSmile.Services.AuthHelper;
using static FreeSmile.Services.Helper;

namespace FreeSmile.Services
{
    public class CommonService : ICommonService
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IStringLocalizer<UsersController> _localizer;
        private readonly FreeSmileContext _context;

        public CommonService(ILogger<UsersController> logger, FreeSmileContext context, IStringLocalizer<UsersController> localizer)
        {
            _logger = logger;
            _context = context;
            _localizer = localizer;
        }

        public async Task DeletePost(int id)
        {
            await _context.Database.ExecuteSqlRawAsync($"dbo.DeletePost {id};");
        }

        public async Task<RegularResponse> AddReviewAsync(ReviewDto value, int user_id)
        {
            try
            {
                var oldReview = await _context.Reviews.Where(x => x.ReviewerId == user_id).FirstOrDefaultAsync();

                if (oldReview is not null)
                {
                    _context.Reviews.Remove(oldReview);
                    await _context.SaveChangesAsync();
                }

                await _context.Reviews.AddAsync(
                    new Review()
                    {
                        Rating = (byte)value.Rating!,
                        Opinion = value.Opinion,
                        ReviewerId = user_id
                    });
                await _context.SaveChangesAsync();

                string message = oldReview is null ? "ReviewAddedSuccessfully" : "ReviewEditedSuccessfully";
                return RegularResponse.Success(message: _localizer[message]);
            }
            catch (Exception ex)
            {
                _logger.LogError("{Message}", ex.Message);

                return RegularResponse.UnknownError(_localizer);
            }
        }

        public async Task<RegularResponse> DeleteReviewAsync(int user_id)
        {
            try
            {
                var review = await _context.Reviews.Where(x => x.ReviewerId == user_id).FirstOrDefaultAsync();

                if (review is not null)
                {
                    _context.Reviews.Remove(review);
                    await _context.SaveChangesAsync();
                }
                return RegularResponse.Success(message: _localizer["ReviewDeletedSuccessfully"]);
            }
            catch (Exception ex)
            {
                _logger.LogError("{Message}", ex.Message);

                return RegularResponse.UnknownError(_localizer);
            }
        }
    }
}
