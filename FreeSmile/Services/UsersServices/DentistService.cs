using FreeSmile.ActionFilters;
using FreeSmile.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using static FreeSmile.Services.Helper;
using static FreeSmile.Services.DirectoryHelper;
using FreeSmile.DTOs.Settings;
using FreeSmile.DTOs.Auth;
using FreeSmile.DTOs.Posts;
using System.Globalization;
using Humanizer;

namespace FreeSmile.Services
{
    public class DentistService : IDentistService
    {
        private readonly ILogger<DentistService> _logger;
        private readonly IStringLocalizer<DentistService> _localizer;
        private readonly FreeSmileContext _context;
        private readonly IUserService _userService;
        private readonly ICommonService _commonService;

        public DentistService(ILogger<DentistService> logger, FreeSmileContext context, IStringLocalizer<DentistService> localizer, IUserService userService, ICommonService commonService)
        {
            _logger = logger;
            _context = context;
            _localizer = localizer;
            _userService = userService;
            _commonService = commonService;
        }

        public async Task<RegularResponse> AddVerificationRequestAsync(VerificationDto verificationDto, int ownerId)
        {
            if (await _context.VerificationRequests.AnyAsync(v => v.OwnerId == ownerId))
                throw new GeneralException(_localizer["AlreadyRequested"]);

            var userDir = GetVerificationPathUser(ownerId);
            if (!Directory.Exists(userDir))
            {
                Directory.CreateDirectory(userDir);
            }

            var natPath = GetVerificationImgPath(ownerId, VerificationType.Nat);
            var proofPath = GetVerificationImgPath(ownerId, VerificationType.Proof);

            try
            {
                byte[] natImage;
                using (var memoryStream = new MemoryStream())
                {
                    await verificationDto.NatIdPhoto.CopyToAsync(memoryStream);
                    natImage = memoryStream.ToArray();
                }
                var natExt = Path.GetExtension(verificationDto.NatIdPhoto.FileName);
                var encoder = ExtensionToEncoder(natExt);
                using (var image = Image.Load(natImage))
                {
                    await image.SaveAsync(natPath, encoder);
                }

                byte[] proofImage;
                using (var memoryStream = new MemoryStream())
                {
                    await verificationDto.ProofOfDegreePhoto.CopyToAsync(memoryStream);
                    proofImage = memoryStream.ToArray();
                }
                var proofExt = Path.GetExtension(verificationDto.ProofOfDegreePhoto.FileName);
                encoder = ExtensionToEncoder(proofExt);
                using (var image = Image.Load(proofImage))
                {
                    await image.SaveAsync(proofPath, encoder);
                }

                await _context.AddAsync(
                    new VerificationRequest()
                    {
                        OwnerId = ownerId,
                        DegreeRequested = (int)verificationDto.DegreeRequested!,
                        UniversityRequested = (int)verificationDto.UniversityRequested!
                    });

                await _context.SaveChangesAsync();

                return RegularResponse.Success(message: _localizer["VerificationRequestSuccess"], nextPage: Pages.pendingVerificationAcceptance.ToString());

            }

            catch (Exception ex) when (ex is UnknownImageFormatException
                                    || ex is InvalidImageContentException
                                    || ex is NotSupportedException)
            {
                if (Directory.Exists(userDir))
                    Directory.Delete(userDir, true);

                throw new GeneralException(_localizer["ImagesOnly", _localizer["SelectedPic"]]);
            }
            catch (Exception)
            {
                if (Directory.Exists(userDir))
                    Directory.Delete(userDir, true);

                throw;
            }
        }

        public async Task<RegularResponse> DeleteVerificationRequestAsync(int user_id_int)
        {
            var CurrentRequest = await _context.VerificationRequests.FirstOrDefaultAsync(x => x.OwnerId == user_id_int);
            if (CurrentRequest is null)
                throw new NotFoundException(_localizer["NotFound", _localizer["request"]]);

            _context.Remove(CurrentRequest);
            await _context.SaveChangesAsync();

            var userDir = GetVerificationPathUser(user_id_int);
            if (Directory.Exists(userDir))
                Directory.Delete(userDir, true);

            return RegularResponse.Success(message: _localizer["DeletedRequest"],
                                            nextPage: Pages.verifyDentist.ToString());
        }
        public async Task<GetDentistSettingsDto> GetSettingsAsync(int user_id)
        {
            var lang = Thread.CurrentThread.CurrentCulture.Name;
            GetCommonSettingsDto dto = await _commonService.GetCommonSettingsAsync(user_id);
            var dentist = await _context.Dentists.AsNoTracking()
                                            .Select(x => new GetDentistSettingsDto()
                                            {
                                                UserId = dto.UserId,
                                                FullName = dto.FullName,
                                                Username = dto.Username,
                                                Phone = dto.Phone,
                                                Email = dto.Email,
                                                Birthdate = dto.Birthdate,
                                                VisibleMail = dto.VisibleMail,
                                                VisibleContact = dto.VisibleContact,
                                                ProfilePicture = dto.ProfilePicture,
                                                AcademicDegree = x.CurrentDegreeNavigation.Lang(lang),
                                                University = x.CurrentUniversityNavigation.Lang(lang),
                                                Bio = x.Bio,
                                                FbUsername = x.FbUsername,
                                                LinkedUsername = x.LinkedUsername,
                                                GScholarUsername = x.GScholarUsername,
                                                ResearchGateUsername = x.ResearchGateUsername,
                                            })
                                            .FirstOrDefaultAsync(x => x.UserId == user_id);
            dentist!.HasPendingVerificationRequest = await _context.VerificationRequests.AnyAsync(x => x.OwnerId == user_id);
            return dentist;
        }

        public async Task<GetDentistSettingsDto> GetPublicSettingsAsync(int auth_user_id, int other_user_id)
        {
            if (auth_user_id != 0)
            {
                // Authenticated user asking for other user settings
                var user1 = await _context.Users.AsNoTracking().Select(x => new { x.Id, x.Suspended }).FirstOrDefaultAsync(x => x.Id == auth_user_id);
                if (user1?.Suspended == true)
                    throw new GeneralException(_localizer["UserSuspended"]);

                if (await _commonService.CanUsersCommunicateAsync(auth_user_id, other_user_id) == false)
                    throw new GeneralException(_localizer["personnotavailable"]);
            }

            var settings = await GetSettingsAsync(other_user_id);
            settings.HidePrivate();

            return settings;
        }

        public async Task<GetDentistSettingsDto> UpdateSettingsAsync(SetDentistSettingsDto settings, int user_id)
        {
            await _commonService.UpdateCommonSettingsAsync(settings, user_id);
            Dentist? dentist = await _context.Dentists.FindAsync(user_id);

            dentist!.Bio = settings.Bio;
            dentist.FbUsername = settings.FbUsername;
            dentist.LinkedUsername = settings.LinkedUsername;
            dentist.GScholarUsername = settings.GScholarUsername;
            dentist.ResearchGateUsername = settings.ResearchGateUsername;
            await _context.SaveChangesAsync();

            return await GetSettingsAsync(user_id);
        }

        public async Task<List<GetCaseDto>> GetPatientsCasesAsync(int user_id, int size, int[] previouslyFetched, int gov_id, int case_type_id)
        {
            bool isEnglish = Thread.CurrentThread.CurrentCulture.Name == "en";
            bool allTypes = case_type_id == 0;
            // Default governorate = dentist univeristy location
            if (gov_id == 0)
            {
                var dentist = await _context.Dentists.AsNoTracking()
                                                     .Select(x =>
                                                        new
                                                        {
                                                            x.DentistId,
                                                            gov_id = x.CurrentUniversityNavigation.Gov.GovId
                                                        }).FirstOrDefaultAsync(x => x.DentistId == user_id);
                gov_id = dentist!.gov_id;
            }

            var excluded_user_ids = await _commonService.GetUserEnemiesAsync(user_id);

            var result = from home in _context.DentistHomeViews.AsNoTracking()
                         where !excluded_user_ids.Contains(home.UserId)
                         where home.GovId == gov_id
                         where allTypes || home.CaseTypeId == case_type_id
                         where !previouslyFetched.Contains(home.PostId)
                         orderby home.TimeUpdated ?? home.TimeWritten descending
                         select new GetCaseDto
                         {
                             IsOwner = home.UserId == user_id,
                             UserInfo = new()
                             {
                                 UserId = home.UserId,
                                 FullName = home.FullName,
                                 Username = home.Username,
                                 ProfilePicture = null,
                             },
                             DentistInfo = home.AcademicDegreeEn == null ? null : new()
                             {
                                 AcademicDegree = isEnglish ? home.AcademicDegreeEn : home.AcademicDegreeAr!,
                                 University = isEnglish ? home.CurrentUnivrsityEn! : home.CurrentUnivrsityAr!,
                             },
                             PostId = home.PostId,
                             Title = home.Title,
                             Body = home.Body,
                             TimeWritten = home.TimeWritten.Humanize(default, default, CultureInfo.CurrentCulture),
                             TimeUpdated = home.TimeUpdated == null ? null : home.TimeUpdated.Humanize(default, default, CultureInfo.CurrentCulture),
                             Written = (DateTime)home.TimeWritten!,
                             Updated = home.TimeUpdated,
                             Images = null,
                             Phone = home.Phone,
                             Governorate = isEnglish ? home.GovNameEn : home.GovNameAr,
                             CaseType = isEnglish ? home.CaseTypeEn : home.CaseTypeAr,
                         };

            var list = result.Take(size).ToList();
            await Parallel.ForEachAsync(list, async (post, cancellationToken) =>
            {
                post.UserInfo.ProfilePicture = await _commonService.GetProfilePictureDangerousAsync(post.UserInfo.UserId, 1);
                post.Images = await _commonService.GetPostImagesAsync(post.PostId);
            });

            return list;
        }

        public async Task<int> AddSharingAsync(AddSharingDto value, int user_id)
        {
            int gov_id = value.GovernorateId;
            if (value.GovernorateId == 0)
            {
                gov_id = (await _context.Dentists.AsNoTracking()
                                                 .Select(x => new
                                                 {
                                                     x.DentistId,
                                                     x.CurrentUniversityNavigation.GovId
                                                 }).FirstOrDefaultAsync(x => x.DentistId == user_id))!.GovId;
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var post_id = await _commonService.AddPostAsync(value, user_id);

                var sharing = new SharingPatient()
                {
                    SharingId = post_id,
                    CaseTypeId = (int)value.CaseTypeId!,
                    GovernateId = gov_id,
                    PatientPhoneNumber = value.PatientPhone
                };
                await _context.AddAsync(sharing);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return sharing.SharingId;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> AddListingAsync(AddListingDto value, int user_id)
        {
            int gov_id = value.GovernorateId;
            if (value.GovernorateId == 0)
            {
                gov_id = (await _context.Dentists.AsNoTracking()
                                                 .Select(x => new
                                                 {
                                                     x.DentistId,
                                                     x.CurrentUniversityNavigation.GovId
                                                 }).FirstOrDefaultAsync(x => x.DentistId == user_id))!.GovId;
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var post_id = await _commonService.AddPostAsync(value, user_id);

                var listing = new Listing()
                {
                    ListingId = post_id,
                    CatId = (int)value.ListingCategoryId!,
                    GovernateId = gov_id,
                    Price = (decimal)value.Price!
                };
                await _context.AddAsync(listing);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return listing.ListingId;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> AddArticleAsync(AddArticleDto value, int user_id)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var post_id = await _commonService.AddPostAsync(value, user_id);

                var article = new Article()
                {
                    ArticleId = post_id,
                    CatId = (int)value.ArticleCategoryId!,
                };
                await _context.AddAsync(article);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return article.ArticleId;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<RegularResponse> UpdateSharingAsync(UpdateSharingDto value, int user_id)
        {
            SharingPatient? sharing = await _context.SharingPatients.Include(x => x.Sharing)
                                              .FirstOrDefaultAsync(x => x.SharingId == (int)value.post_id! && x.Sharing.WriterId == user_id);
            if (sharing is null)
                throw new NotFoundException(_localizer["notfound", _localizer["thispost"]]);


            int gov_id = value.GovernorateId;
            if (value.GovernorateId == 0)
            {
                gov_id = (await _context.Dentists.AsNoTracking()
                                                 .Select(x => new
                                                 {
                                                     x.DentistId,
                                                     x.CurrentUniversityNavigation.GovId
                                                 }).FirstOrDefaultAsync(x => x.DentistId == user_id))!.GovId;
            }

            sharing.GovernateId = gov_id;
            sharing.CaseTypeId = (int)value.CaseTypeId!;
            sharing.PatientPhoneNumber = value.PatientPhone;
            
            sharing.Sharing.Title = value.Title;
            sharing.Sharing.Body = value.Body;
            sharing.Sharing.TimeUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RegularResponse.Success(message: _localizer["PostEditedSuccess"]);
        }

        public async Task<RegularResponse> UpdateListingAsync(UpdateListingDto value, int user_id)
        {
            Listing? listing = await _context.Listings.Include(x => x.ListingNavigation)
                                              .FirstOrDefaultAsync(x => x.ListingId == (int)value.post_id! && x.ListingNavigation.WriterId == user_id);
            if (listing is null)
                throw new NotFoundException(_localizer["notfound", _localizer["thispost"]]);


            int gov_id = value.GovernorateId;
            if (value.GovernorateId == 0)
            {
                gov_id = (await _context.Dentists.AsNoTracking()
                                                 .Select(x => new
                                                 {
                                                     x.DentistId,
                                                     x.CurrentUniversityNavigation.GovId
                                                 }).FirstOrDefaultAsync(x => x.DentistId == user_id))!.GovId;
            }

            listing.GovernateId = gov_id;
            listing.CatId = (int)value.ListingCategoryId!;
            listing.Price = (decimal)value.Price!;
            
            listing.ListingNavigation.Title = value.Title;
            listing.ListingNavigation.Body = value.Body;
            listing.ListingNavigation.TimeUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RegularResponse.Success(message: _localizer["PostEditedSuccess"]);
        }

        public async Task<RegularResponse> UpdateArticleAsync(UpdateArticleDto value, int user_id)
        {
            Article? article = await _context.Articles.Include(x => x.ArticleNavigation)
                                                 .FirstOrDefaultAsync(x => x.ArticleId == (int)value.post_id! && x.ArticleNavigation.WriterId == user_id);
            if (article is null)
                throw new NotFoundException(_localizer["notfound", _localizer["thispost"]]);


            article.CatId = (int)value.ArticleCategoryId!;
            
            article.ArticleNavigation.Title = value.Title;
            article.ArticleNavigation.Body = value.Body;
            article.ArticleNavigation.TimeUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RegularResponse.Success(message: _localizer["PostEditedSuccess"]);
        }

        public async Task<List<GetListingDto>> GetListingsAsync(int user_id, int size, int[] previouslyFetched, int gov_id, int listingCategoryId, byte sortBy)
        {
            bool isEnglish = Thread.CurrentThread.CurrentCulture.Name == "en";
            bool allCats = listingCategoryId == 0;
            // Default governorate = dentist univeristy location
            if (gov_id == 0)
            {
                var dentist = await _context.Dentists.AsNoTracking()
                                                     .Select(x =>
                                                        new
                                                        {
                                                            x.DentistId,
                                                            gov_id = x.CurrentUniversityNavigation.Gov.GovId
                                                        }).FirstOrDefaultAsync(x => x.DentistId == user_id);
                gov_id = dentist!.gov_id;
            }

            var excluded_user_ids = await _commonService.GetUserEnemiesAsync(user_id);

            var result = from home in _context.ListingsViews.AsNoTracking()
                         where !excluded_user_ids.Contains(home.UserId)
                         where home.GovId == gov_id
                         where allCats || home.ProductCatId == listingCategoryId
                         where !previouslyFetched.Contains(home.PostId)
                         select new GetListingDto
                         {
                             IsOwner = home.UserId == user_id,
                             UserInfo = new()
                             {
                                 UserId = home.UserId,
                                 FullName = home.FullName,
                                 Username = home.Username,
                                 ProfilePicture = null,
                             },
                             DentistInfo = new()
                             {
                                 AcademicDegree = isEnglish ? home.AcademicDegreeEn : home.AcademicDegreeAr!,
                                 University = isEnglish ? home.CurrentUnivrsityEn! : home.CurrentUnivrsityAr!,
                             },
                             PostId = home.PostId,
                             Title = home.Title,
                             Body = home.Body,
                             TimeWritten = home.TimeWritten.Humanize(default, default, CultureInfo.CurrentCulture),
                             TimeUpdated = home.TimeUpdated == null ? null : home.TimeUpdated.Humanize(default, default, CultureInfo.CurrentCulture),
                             Written = (DateTime)home.TimeWritten!,
                             Updated = home.TimeUpdated,
                             Images = null,
                             Phone = home.Phone,
                             Governorate = isEnglish ? home.GovNameEn : home.GovNameAr,
                             ProductCategory = isEnglish ? home.ProductCatEn : home.ProductCatAr,
                             Price = home.Price
                         };
            
            //0 : Date Desc, 1 : Date Asc, 2 : Price Asc, 3 : Price Desc
            result = sortBy switch
            {
                0 => result.OrderByDescending(x => x.Updated ?? x.Written),
                1 => result.OrderBy(x => x.Updated ?? x.Written),
                2 => result.OrderBy(x => x.Price),
                3 => result.OrderByDescending(x => x.Price),
                _ => result.OrderByDescending(x => x.Updated ?? x.Written),
            };
            
            var list = result.Take(size).ToList();
            await Parallel.ForEachAsync(list, async (post, cancellationToken) =>
            {
                post.UserInfo.ProfilePicture = await _commonService.GetProfilePictureDangerousAsync(post.UserInfo.UserId, 1);
                post.Images = await _commonService.GetPostImagesAsync(post.PostId);
            });

            return list;
        }

        public async Task<List<GetArticleDto>> GetArticlesAsync(int user_id, int size, int[] previouslyFetched, int articleCategoryId, byte sortBy)
        {
            bool isEnglish = Thread.CurrentThread.CurrentCulture.Name == "en";
            bool allCats = articleCategoryId == 0;
            var excluded_user_ids = await _commonService.GetUserEnemiesAsync(user_id);

            var result = from home in _context.ArticlesViews.AsNoTracking()
                         where !excluded_user_ids.Contains(home.UserId)
                         where allCats || home.ArticleCatId== articleCategoryId
                         where !previouslyFetched.Contains(home.PostId)
                         select new GetArticleDto
                         {
                             IsOwner = home.UserId == user_id,
                             UserInfo = new()
                             {
                                 UserId = home.UserId,
                                 FullName = home.FullName,
                                 Username = home.Username,
                                 ProfilePicture = null,
                             },
                             DentistInfo = new()
                             {
                                 AcademicDegree = isEnglish ? home.AcademicDegreeEn : home.AcademicDegreeAr!,
                                 University = isEnglish ? home.CurrentUnivrsityEn! : home.CurrentUnivrsityAr!,
                             },
                             PostId = home.PostId,
                             Title = home.Title,
                             Body = home.Body,
                             TimeWritten = home.TimeWritten.Humanize(default, default, CultureInfo.CurrentCulture),
                             TimeUpdated = home.TimeUpdated == null ? null : home.TimeUpdated.Humanize(default, default, CultureInfo.CurrentCulture),
                             Written = (DateTime)home.TimeWritten!,
                             Updated = home.TimeUpdated,
                             Images = null,
                             Phone = home.Phone,
                             ArticleCateogry = isEnglish ? home.ArticleCatEn : home.ArticleCatAr,
                             Likes = home.Likes ?? 0,
                             Comments = home.Comments ?? 0
                         };

            // 0 : Date Desc, 1 : Date Asc, 2 : Likes Desc, 3 : Comments Desc 
            result = sortBy switch
            {
                0 => result.OrderByDescending(x => x.Updated ?? x.Written),
                1 => result.OrderBy(x => x.Updated ?? x.Written),
                2 => result.OrderByDescending(x => x.Likes),
                3 => result.OrderByDescending(x => x.Comments),
                _ => result.OrderByDescending(x => x.Updated ?? x.Written),
            };

            var list = result.Take(size).ToList();
            await Parallel.ForEachAsync(list, async (post, cancellationToken) =>
            {
                post.UserInfo.ProfilePicture = await _commonService.GetProfilePictureDangerousAsync(post.UserInfo.UserId, 1);
                post.Images = await _commonService.GetPostImagesAsync(post.PostId);
            });

            return list;
        }
    }
}

