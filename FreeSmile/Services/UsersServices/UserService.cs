using FreeSmile.ActionFilters;
using FreeSmile.Controllers;
using FreeSmile.DTOs;
using FreeSmile.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using static FreeSmile.Services.AuthHelper;
using static FreeSmile.Services.EmailService;
using static FreeSmile.Services.Helper;

namespace FreeSmile.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IStringLocalizer<UserService> _localizer;
        private readonly FreeSmileContext _context;
        private readonly ICommonService _commonService;

        public UserService(ILogger<UserService> logger, FreeSmileContext context, IStringLocalizer<UserService> localizer, ICommonService commonService)
        {
            _logger = logger;
            _context = context;
            _localizer = localizer;
            _commonService = commonService;
        }
        public async Task<RegularResponse> AddUserAsync(UserRegisterDto userDto)
        {
            // Allow duplicate emails if user not verified yet
            _context.Users.RemoveRange(_context.Users.Where(x => x.Email == userDto.Email && x.IsVerified == false));
            await _context.SaveChangesAsync();

            // Allow duplicate phones if user not verified yet
            _context.Users.RemoveRange(_context.Users.Where(x => x.Phone == userDto.Phone && x.IsVerified == false));
            await _context.SaveChangesAsync();


            var salt = GenerateSalt();
            string storedPass = StorePassword(userDto.Password, salt);

            var user = new User()
            {
                Username = userDto.Username,
                Email = userDto.Email,
                Password = storedPass,
                Salt = salt,
                Phone = userDto.Phone,
                Fullname = userDto.Fullname,
                Gender = userDto.Gender,
                Bd = userDto.Birthdate is null ? null : DateTime.Parse(userDto.Birthdate)
            };

            await _context.AddAsync(user);
            await _context.SaveChangesAsync();

            return new RegularResponse() { Id = user.Id };
        }

        public async Task<RegularResponse> AddPatientAsync(UserRegisterDto userDto, IResponseCookies cookies)
        {
            RegularResponse response;
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                response = await AddUserAsync(userDto);

                var patient = new Patient()
                {
                    PatientId = response.Id
                };
                await _context.AddAsync(patient);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var tokenAge = MyConstants.REGISTER_TOKEN_AGE;
                string token = GetToken(patient.PatientId, tokenAge, Role.Patient);
                cookies.Append(MyConstants.AUTH_COOKIE_KEY, token, new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.None, MaxAge = tokenAge, Secure = true });

                response = RegularResponse.Success(
                    id: patient.PatientId,
                    token: token,
                    message: _localizer["RegisterSuccess"],
                    nextPage: Pages.verifyEmail.ToString()
                );
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
            return response;
        }

        public async Task<RegularResponse> AddDentistAsync(UserRegisterDto userDto, IResponseCookies cookies)
        {
            RegularResponse response;

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                response = await AddUserAsync(userDto);

                var dentist = new Dentist()
                {
                    DentistId = response.Id
                    // current university and current degree are defaulted at first
                };
                await _context.AddAsync(dentist);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var tokenAge = MyConstants.REGISTER_TOKEN_AGE;
                string token = GetToken(dentist.DentistId, tokenAge, Role.Dentist);
                cookies.Append(MyConstants.AUTH_COOKIE_KEY, token, new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.None, MaxAge = tokenAge, Secure = true });

                response = RegularResponse.Success(
                    id: dentist.DentistId,
                    token: token,
                    message: _localizer["RegisterSuccess"],
                    nextPage: Pages.verifyEmail.ToString());
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
            return response;

        }

        public async Task<RegularResponse> AddAdminAsync(UserRegisterDto userDto, IResponseCookies cookies)
        {
            RegularResponse response;
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                response = await AddUserAsync(userDto);

                var admin = new Admin()
                {
                    AdminId = response.Id
                };
                await _context.AddAsync(admin);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                response = RegularResponse.Success(
                    id: admin.AdminId,
                    message: _localizer["RegisterSuccess"],
                    nextPage: Pages.same.ToString() // only superadmins can register admins so they will not verify them too && also no token is sent
                );
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
            return response;

        }

        public async Task<RegularResponse> VerifyAccount(string otp, int user_id)
        {
            User? user = await _context.Users.FindAsync(user_id);

            Role role = await _commonService.GetCurrentRole(user!.Id);

            if (user.IsVerified)
                throw new GeneralException(
                    _localizer["UserAlreadyVerified"],
                    Pages.home.ToString() + role.ToString()
                );

            if (user.Otp != otp)
                throw new GeneralException(
                    _localizer["OtpNotMatch"]
                );

            if (user.OtpExp < DateTime.UtcNow)
                throw new GeneralException(
                    _localizer["OtpExpired"]
                );

            user.IsVerified = true;
            user.OtpExp = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            string nextPage = (role == Role.Dentist) ? Pages.verifyDentist.ToString() : Pages.home.ToString() + role.ToString();


            return RegularResponse.Success(
                message: _localizer["EmailVerificationSuccess"],
                nextPage: nextPage
            );
        }

        public async Task<RegularResponse> Login(UserLoginDto value, IResponseCookies cookies)
        {
            // Check email or username
            // Check password
            // Check redirect if not verified
            User? user = await _context.Users.FirstOrDefaultAsync(x => x.Email == value.UsernameOrEmail || x.Username == value.UsernameOrEmail);
            if (user is null
             || StorePassword(value.Password, user.Salt) != user.Password)
                throw new GeneralException(_localizer["IncorrectCreds"]);

            if (user.Suspended)
                throw new GeneralException(_localizer["UserSuspended"]);

            Role role = await _commonService.GetCurrentRole(user.Id);

            TimeSpan loginTokenAge = MyConstants.LOGIN_TOKEN_AGE;

            string token = GetToken(user.Id, loginTokenAge, role);
            cookies.Append(MyConstants.AUTH_COOKIE_KEY, token, new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.None, MaxAge = loginTokenAge, Secure = true });

            string nextPage = Pages.home.ToString() + role.ToString();

            if (role == Role.Dentist)
            {
                var dentist = await _context.Dentists.FindAsync(user.Id);
                if (dentist.IsVerifiedDentist == false)
                {
                    if (!_context.VerificationRequests.Any(req => req.OwnerId == user.Id))
                    {
                        nextPage = Pages.verifyDentist.ToString();
                    }
                    else
                    {
                        nextPage = Pages.pendingVerificationAcceptance.ToString();
                    }
                }
            }

            if (user.IsVerified == false)
                nextPage = Pages.verifyEmail.ToString();

            return RegularResponse.Success(
                token: token,
                nextPage: nextPage,
                message: _localizer["LoginSuccess"]
            );
        }

        public async Task<RegularResponse> ChangePassword(ResetPasswordDto request)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(x => x.Email == request.UsernameOrEmail || x.Username == request.UsernameOrEmail);
            if (user is null)
                throw new GeneralException(
                    _localizer["UserNotFound"],
                    Pages.login.ToString()
                );

            if (user.Suspended)
                throw new GeneralException(_localizer["UserSuspended"]);

            if (request.Otp != user.Otp)
                throw new GeneralException(_localizer["OtpNotMatch"]);

            if (user.OtpExp < DateTime.UtcNow)
                throw new GeneralException(_localizer["OtpExpired"]);

            user.Password = StorePassword(request.NewPassword, user.Salt);
            user.OtpExp = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            try
            {
                user.Notifications.Add(new()
                {
                    Temp = await _context.NotificationTemplates.FirstOrDefaultAsync(x => x.TempName == NotificationTemplates.Reset_Password.ToString())
                });
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("{Message}", ex.Message);
            }

            return RegularResponse.Success(
                message: _localizer["PasswordChangedSuccessfully"],
                nextPage: Pages.login.ToString()
            );
        }

        public async Task<RegularResponse> ChangePassword(ChangeKnownPasswordDto request, int user_id)
        {
            User? user = await _context.Users.FindAsync(user_id);

            Role role = await _commonService.GetCurrentRole(user!.Id);

            if (StorePassword(request.CurrentPassword, user.Salt) != user.Password)
                throw new GeneralException(_localizer["IncorrectCurrentPassword"]);

            user.Password = StorePassword(request.NewPassword, user.Salt);
            await _context.SaveChangesAsync();

            try
            {
                user.Notifications.Add(new()
                {
                    Temp = await _context.NotificationTemplates.FirstOrDefaultAsync(x => x.TempName == NotificationTemplates.Changed_Password.ToString())
                });
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("{Message}", ex.Message);
            }

            return RegularResponse.Success(
                    message: _localizer["PasswordChangedSuccessfully"]
            );
        }

        public async Task<RegularResponse> RequestEmailOtp(int user_id)
        {

            User? user = await _context.Users.FindAsync(user_id);
            if (user is null)
                throw new GeneralException(
                    _localizer["UserNotFound"],
                    Pages.login.ToString()
                );

            if (user.Suspended)
                throw new GeneralException(_localizer["UserSuspended"]);

            if (user.IsVerified)
            {
                Role role = await _commonService.GetCurrentRole(user.Id);
                string nextPage = Pages.home.ToString() + role.ToString();
                if (role == Role.Dentist)
                {
                    var dentist = await _context.Dentists.FindAsync(user.Id);
                    if (dentist.IsVerifiedDentist == false)
                    {
                        if (!_context.VerificationRequests.Any(req => req.Owner == dentist))
                        {
                            nextPage = Pages.verifyDentist.ToString();
                        }
                        else
                        {
                            nextPage = Pages.pendingVerificationAcceptance.ToString();
                        }
                    }
                }
                throw new GeneralException(
                    _localizer["emailalreadyverified"],
                    nextPage
                );
            }


            user.Otp = GenerateOtp();
            user.OtpExp = DateTime.UtcNow + MyConstants.OTP_AGE;

            await _context.SaveChangesAsync();

            try
            {
                SendEmail(user.Email,
                        _localizer["otpemailsubject"],
                        MyConstants.otpemailfilename,
                        _localizer["lang"],
                true,
                        user.Username, MyConstants.OTP_AGE.Minutes, user.Otp);
            }
            catch (Exception ex)
            {
                throw new InternalServerException(_localizer["ErrorSendingEmail"]);
            }

            return RegularResponse.Success(
                message: _localizer["SentOtpSuccessfully", user.Email],
                nextPage: Pages.same.ToString()
            );
        }

        public async Task<RegularResponse> RequestEmailOtp(string usernameOrEmail)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(x => x.Email == usernameOrEmail || x.Username == usernameOrEmail);
            if (user is null)
                throw new GeneralException(_localizer["UserNotFound"]);

            if (user.Suspended)
                throw new GeneralException(_localizer["UserSuspended"]);

            user.Otp = GenerateOtp();
            user.OtpExp = DateTime.UtcNow + MyConstants.OTP_AGE;

            await _context.SaveChangesAsync();

            try
            {
                SendEmail(user.Email,
                        _localizer["otpemailsubject"],
                        MyConstants.otpemailfilename,
                        _localizer["lang"],
                true,
                        user.Username, MyConstants.OTP_AGE.Minutes, user.Otp);
            }
            catch (Exception ex)
            {
                throw new InternalServerException(_localizer["ErrorSendingEmail"]);
            }

            return RegularResponse.Success(
                message: _localizer["SentOtpSuccessfully", ObscureEmail(user.Email)],
                nextPage: Pages.same.ToString()
            );
        }

        public async Task<RegularResponse> RedirectToHome(int user_id)
        {
            Role role = await _commonService.GetCurrentRole(user_id);
            return RegularResponse.Success(
                nextPage: Pages.home.ToString() + role.ToString()
            );
        }

        public async Task<RegularResponse> DeleteMyAccount(DeleteMyAccountDto value, int user_id, IResponseCookies cookies)
        {
            User? user = await _context.Users.FindAsync(user_id);

            if (StorePassword(value.CurrentPassword, user.Salt) != user.Password)
                throw new GeneralException(_localizer["WrongPassword"]);

            // Delete All his posts first
            var postsIds = _context.Posts.Where(x => x.WriterId == user_id).Select(x => x.PostId).ToList();
            foreach (var id in postsIds)
            {
                await _commonService.DeletePost(id);
            }
            
            var profilePicPath = DirectoryHelper.GetProfilePicturesUser(user_id);
            if (Directory.Exists(profilePicPath))
                Directory.Delete(profilePicPath, true);

            _context.Remove(user);
            await _context.SaveChangesAsync();

            cookies.Append(MyConstants.AUTH_COOKIE_KEY, string.Empty, new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.None, Expires = DateTime.Now.AddDays(-5), Secure = true });

            return RegularResponse.Success(
                message: _localizer["AccountDeleted"],
                nextPage: Pages.login.ToString()
            );
        }
    }
}

