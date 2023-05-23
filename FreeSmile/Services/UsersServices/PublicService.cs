using DTOs;
using FreeSmile.ActionFilters;
using FreeSmile.DTOs.Posts;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace FreeSmile.Services
{
    public class PublicService : IPublicService
    {
        private readonly ILogger<PublicService> _logger;
        private readonly IStringLocalizer<PublicService> _localizer;
        private readonly FreeSmileContext _context;
        private readonly ICommonService _commonService;

        public PublicService(ILogger<PublicService> logger, FreeSmileContext context, IStringLocalizer<PublicService> localizer, ICommonService commonService)
        {
            _logger = logger;
            _context = context;
            _localizer = localizer;
            _commonService = commonService;
        }

        public async Task<GetPostDto> GetPostAsync(int auth_user_id, int postId)
        {
            if (!await _context.Posts.AnyAsync(x => x.PostId == postId))
                throw new NotFoundException(_localizer["NotFound", _localizer["thispost"]]);

            bool isEnglish = Thread.CurrentThread.CurrentCulture.Name == "en";
            GetPostDto? result = null;

            if (await _context.Cases.AnyAsync(x => x.CaseId == postId))
            {
                result = await (from cas1 in _context.Cases.AsNoTracking()
                                join post in _context.Posts.AsNoTracking() on cas1.CaseId equals post.PostId
                                join user in _context.Users.AsNoTracking() on post.WriterId equals user.Id
                                join gov in _context.Governates.AsNoTracking() on cas1.GovernateId equals gov.GovId
                                join casetype in _context.CaseTypes.AsNoTracking() on cas1.CaseTypeId equals casetype.CaseTypeId

                                join dentist in _context.Dentists.AsNoTracking() on user.Id equals dentist.DentistId into dentistsJoin
                                from d in dentistsJoin.DefaultIfEmpty()

                                join deg in _context.AcademicDegrees.AsNoTracking() on d.CurrentDegree equals deg.DegId into degJoin
                                from degree in degJoin.DefaultIfEmpty()

                                join univ in _context.Universities.AsNoTracking() on d.CurrentUniversity equals univ.UniversityId into universities_join
                                from university in universities_join.DefaultIfEmpty()

                                select new GetPostDto
                                {
                                    IsCase = true,
                                    IsOwner = auth_user_id == user.Id,
                                    UserInfo = new()
                                    {
                                        UserId = user.Id,
                                        FullName = user.Fullname,
                                        Username = user.Username,
                                        ProfilePicture = null,
                                    },
                                    DentistInfo = degree.NameEn == null ? null : new()
                                    {
                                        AcademicDegree = isEnglish ? degree.NameEn : degree.NameAr,
                                        University = isEnglish ? university.NameEn : university.NameAr,
                                    },
                                    PostId = post.PostId,
                                    Title = post.Title,
                                    Body = post.Body,
                                    TimeWritten = post.TimeWritten.Humanize(default, default, CultureInfo.CurrentCulture),
                                    TimeUpdated = post.TimeUpdated == null ? null : post.TimeUpdated.Humanize(default, default, CultureInfo.CurrentCulture),
                                    Written = (DateTime)post.TimeWritten!,
                                    Updated = post.TimeUpdated,
                                    Images = null,
                                    Phone = user.VisibleContact ? user.Phone : null,
                                    Governorate = isEnglish ? gov.NameEn : gov.NameAr,
                                    Category = isEnglish ? casetype.NameEn : casetype.NameAr,
                                }).FirstOrDefaultAsync(x => x.PostId == postId);
            }
            else if (await _context.SharingPatients.AnyAsync(x => x.SharingId == postId))
            {
                result = await (from sharing in _context.SharingPatients.AsNoTracking()
                                join post in _context.Posts.AsNoTracking() on sharing.SharingId equals post.PostId
                                join user in _context.Users.AsNoTracking() on post.WriterId equals user.Id
                                join gov in _context.Governates.AsNoTracking() on sharing.GovernateId equals gov.GovId
                                join casetype in _context.CaseTypes.AsNoTracking() on sharing.CaseTypeId equals casetype.CaseTypeId
                                join dentist in _context.Dentists.AsNoTracking() on user.Id equals dentist.DentistId
                                join degree in _context.AcademicDegrees.AsNoTracking() on dentist.CurrentDegree equals degree.DegId
                                join university in _context.Universities.AsNoTracking() on dentist.CurrentUniversity equals university.UniversityId
                                select new GetPostDto
                                {
                                    IsCase = true,
                                    IsOwner = auth_user_id == user.Id,
                                    UserInfo = new()
                                    {
                                        UserId = user.Id,
                                        FullName = user.Fullname,
                                        Username = user.Username,
                                        ProfilePicture = null,
                                    },
                                    DentistInfo = degree.NameEn == null ? null : new()
                                    {
                                        AcademicDegree = isEnglish ? degree.NameEn : degree.NameAr,
                                        University = isEnglish ? university.NameEn : university.NameAr,
                                    },
                                    PostId = post.PostId,
                                    Title = post.Title,
                                    Body = post.Body,
                                    TimeWritten = post.TimeWritten.Humanize(default, default, CultureInfo.CurrentCulture),
                                    TimeUpdated = post.TimeUpdated == null ? null : post.TimeUpdated.Humanize(default, default, CultureInfo.CurrentCulture),
                                    Written = (DateTime)post.TimeWritten!,
                                    Updated = post.TimeUpdated,
                                    Images = null,
                                    Phone = sharing.PatientPhoneNumber,
                                    Governorate = isEnglish ? gov.NameEn : gov.NameAr,
                                    Category = isEnglish ? casetype.NameEn : casetype.NameAr,
                                }).FirstOrDefaultAsync(x => x.PostId == postId);
            }
            else if (await _context.Listings.AnyAsync(x => x.ListingId == postId))
            {
                result = await (from listing in _context.Listings.AsNoTracking()
                                join post in _context.Posts.AsNoTracking() on listing.ListingId equals post.PostId
                                join user in _context.Users.AsNoTracking() on post.WriterId equals user.Id
                                join gov in _context.Governates.AsNoTracking() on listing.GovernateId equals gov.GovId
                                join productCat in _context.ProductCats.AsNoTracking() on listing.CatId equals productCat.ProductCatId
                                join dentist in _context.Dentists.AsNoTracking() on user.Id equals dentist.DentistId
                                join degree in _context.AcademicDegrees.AsNoTracking() on dentist.CurrentDegree equals degree.DegId
                                join university in _context.Universities.AsNoTracking() on dentist.CurrentUniversity equals university.UniversityId
                                select new GetPostDto
                                {
                                    IsListing = true,
                                    Price = listing.Price,
                                    IsOwner = auth_user_id == user.Id,
                                    UserInfo = new()
                                    {
                                        UserId = user.Id,
                                        FullName = user.Fullname,
                                        Username = user.Username,
                                        ProfilePicture = null,
                                    },
                                    DentistInfo = new()
                                    {
                                        AcademicDegree = isEnglish ? degree.NameEn : degree.NameAr,
                                        University = isEnglish ? university.NameEn : university.NameAr,
                                    },
                                    PostId = post.PostId,
                                    Title = post.Title,
                                    Body = post.Body,
                                    TimeWritten = post.TimeWritten.Humanize(default, default, CultureInfo.CurrentCulture),
                                    TimeUpdated = post.TimeUpdated == null ? null : post.TimeUpdated.Humanize(default, default, CultureInfo.CurrentCulture),
                                    Written = (DateTime)post.TimeWritten!,
                                    Updated = post.TimeUpdated,
                                    Images = null,
                                    Phone = user.VisibleContact ? user.Phone : null,
                                    Governorate = isEnglish ? gov.NameEn : gov.NameAr,
                                    Category = isEnglish ? productCat.NameEn : productCat.NameAr,
                                }).FirstOrDefaultAsync(x => x.PostId == postId);
            }
            else
            {
                result = await _context.ArticlesViews.AsNoTracking()
                                       .Select(article => new GetPostDto
                                       {
                                           IsArticle = true,
                                           Likes = (int)article.Likes!,
                                           Comments = (int)article.Comments!,
                                           IsOwner = auth_user_id == article.UserId,
                                           UserInfo = new()
                                           {
                                               UserId = article.UserId,
                                               FullName = article.FullName,
                                               Username = article.Username,
                                               ProfilePicture = null,
                                           },
                                           DentistInfo = new()
                                           {
                                               AcademicDegree = isEnglish ? article.AcademicDegreeEn : article.AcademicDegreeAr,
                                               University = isEnglish ? article.CurrentUnivrsityEn : article.CurrentUnivrsityAr,
                                           },
                                           PostId = article.PostId,
                                           Title = article.Title,
                                           Body = article.Body,
                                           TimeWritten = article.TimeWritten.Humanize(default, default, CultureInfo.CurrentCulture),
                                           TimeUpdated = article.TimeUpdated == null ? null : article.TimeUpdated.Humanize(default, default, CultureInfo.CurrentCulture),
                                           Written = (DateTime)article.TimeWritten!,
                                           Updated = article.TimeUpdated,
                                           Images = null,
                                           Phone = article.Phone,
                                           Category = isEnglish ? article.ArticleCatEn : article.ArticleCatAr,
                                       })
                                       .FirstOrDefaultAsync(x => x.PostId == postId);
            }

            if (await _commonService.CanUsersCommunicateAsync(auth_user_id, result!.UserInfo.UserId) == false)
                throw new GeneralException(_localizer["personnotavailable"]);

            result.UserInfo.ProfilePicture = await _commonService.GetProfilePictureDangerousAsync(result.UserInfo.UserId, 1);
            result.Images = await _commonService.GetPostImagesAsync(result.PostId);
            return result;
        }

        public async Task<List<CommentDto>> GetArticleCommentsAsync(int auth_user_id, int size, int[] previouslyFetched, int articleId)
        {
            if (articleId <= 0)
                throw new NotFoundException(_localizer["NotFound", _localizer["thispost"]]);

            var result = await _context.Articles.AsNoTracking()
                                   .Select(article => new
                                   {
                                       article.ArticleId,
                                       article.ArticleNavigation.WriterId
                                   })
                                   .FirstOrDefaultAsync(x => x.ArticleId == articleId);

            if (result is null)
                throw new NotFoundException(_localizer["NotFound", _localizer["thispost"]]);

            bool isEnglish = Thread.CurrentThread.CurrentCulture.Name == "en";

            if (await _commonService.CanUsersCommunicateAsync(auth_user_id, result!.WriterId) == false)
                throw new GeneralException(_localizer["personnotavailable"]);

            var enemies = await _commonService.GetUserEnemiesAsync(auth_user_id);

            var list = await _context.Comments
                .Where(x => x.ArticleId == articleId)
                .Where(x => !previouslyFetched.Contains(x.CommentId))
                .Where(x => !enemies.Contains(x.WriterId))
                .Select(x => new CommentDto
                {
                    CommentId = x.CommentId,
                    Written = x.TimeWritten,
                    TimeWritten = x.TimeWritten.Humanize(default, default, CultureInfo.CurrentCulture),
                    Body = x.Body,
                    UserInfo = new()
                    {
                        UserId = x.Writer.Id,
                        FullName = x.Writer.Fullname,
                        Username = x.Writer.Username
                    },
                    DentistInfo = new()
                    {
                        AcademicDegree = isEnglish ?
                                         x.Writer.Dentist.CurrentDegreeNavigation.NameEn :
                                         x.Writer.Dentist.CurrentDegreeNavigation.NameAr,
                        University = isEnglish ?
                                     x.Writer.Dentist.CurrentUniversityNavigation.NameEn :
                                     x.Writer.Dentist.CurrentUniversityNavigation.NameAr
                    }
                }).OrderByDescending(x => x.CommentId).Take(size).ToListAsync();

            await Parallel.ForEachAsync(list, async (post, cancellationToken) =>
            {
                post.UserInfo.ProfilePicture = await _commonService.GetProfilePictureDangerousAsync(post.UserInfo.UserId, 1);
            });
            return list;
        }

        public async Task<List<GetBasicUserInfo>> GetArticleLikesAsync(int auth_user_id, int size, int[] previouslyFetched, int articleId)
        {
            if (articleId <= 0)
                throw new NotFoundException(_localizer["NotFound", _localizer["thispost"]]);

            var result = await _context.Articles.AsNoTracking()
                                   .Select(article => new
                                   {
                                       article.ArticleId,
                                       article.ArticleNavigation.WriterId
                                   })
                                   .FirstOrDefaultAsync(x => x.ArticleId == articleId);

            if (result is null)
                throw new NotFoundException(_localizer["NotFound", _localizer["thispost"]]);

            bool isEnglish = Thread.CurrentThread.CurrentCulture.Name == "en";

            if (await _commonService.CanUsersCommunicateAsync(auth_user_id, result!.WriterId) == false)
                throw new GeneralException(_localizer["personnotavailable"]);

            var enemies = await _commonService.GetUserEnemiesAsync(auth_user_id);

            var list = (await _context.Articles
                .Where(x => x.ArticleId == articleId)
                .Select(x => x.Likers.Select(x => new GetBasicUserInfo
                {
                    UserId = x.Id,
                    Username = x.Username,
                    FullName = x.Fullname
                })
                .Where(x => !previouslyFetched.Contains(x.UserId))
                .Where(x => !enemies.Contains(x.UserId)))
                .Take(size).FirstOrDefaultAsync())!.ToList();

            await Parallel.ForEachAsync(list, async (like, cancellationToken) =>
            {
                like.ProfilePicture = await _commonService.GetProfilePictureDangerousAsync(like.UserId, 1);
            });
            return list;
        }
    }
}
