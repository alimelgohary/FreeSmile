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
        private readonly ILogger<UsersController> _logger;
        private readonly IStringLocalizer<UsersController> _localizer;
        private readonly FreeSmileContext _context;

        public UserService(ILogger<UsersController> logger, FreeSmileContext context, IStringLocalizer<UsersController> localizer)
        {
            _logger = logger;
            _context = context;
            _localizer = localizer;
        }
        public async Task<RegularResponse> AddUserAsync(UserRegisterDto userDto, IResponseCookies cookies)
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

        public async Task<RegularResponse> VerifyAccount(string otp, int user_id)
        {
            try
            {
                User? user = await _context.Users.FindAsync(user_id);

                Role role = await GetCurrentRole(user!.Id);

                if (user.IsVerified)
                    return RegularResponse.BadRequestError(
                        error: _localizer["UserAlreadyVerified"],
                        nextPage: Pages.home.ToString() + role.ToString()
                    );

                if (user.Otp != otp)
                    return RegularResponse.BadRequestError(
                        error: _localizer["OtpNotMatch"]
                    );

                if (user.OtpExp < DateTime.UtcNow)
                    return RegularResponse.BadRequestError(
                        error: _localizer["OtpExpired"]
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
            catch (Exception ex)
            {
                _logger.LogError("{Message}", ex.Message);

                return RegularResponse.UnknownError(_localizer);
            }
        }

        public async Task<RegularResponse> Login(UserLoginDto value, IResponseCookies cookies)
        {
            // Check email or username
            // Check password
            // Check redirect if not verified
            try
            {
                User? user = await _context.Users.Where(x => x.Email == value.UsernameOrEmail || x.Username == value.UsernameOrEmail).FirstOrDefaultAsync();
                if (user is null
                 || StorePassword(value.Password, user.Salt) != user.Password)
                    return RegularResponse.BadRequestError(
                        error: _localizer["IncorrectCreds"]
                    );

                if (user.Suspended)
                    return RegularResponse.BadRequestError(error: _localizer["UserSuspended"]);

                Role role = await GetCurrentRole(user.Id);

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
            catch (Exception ex)
            {
                _logger.LogError("{Message}", ex.Message);

                return RegularResponse.UnknownError(_localizer);
            }
        }
        async Task<Role> GetCurrentRole(int user_id)
        {
            Role role = Role.Patient;

            if (await _context.SuperAdmins.AnyAsync(x => x.SuperAdminId == user_id))
                role = Role.SuperAdmin;

            if (await _context.Dentists.AnyAsync(x => x.DentistId == user_id))
                role = Role.Dentist;

            if (await _context.Patients.AnyAsync(x => x.PatientId == user_id))
                role = Role.Patient;

            if (await _context.Admins.AnyAsync(x => x.AdminId == user_id))
                role = Role.Admin;

            return role;
        }

        public async Task<RegularResponse> ChangePassword(ResetPasswordDto request)
        {
            try
            {
                User? user = await _context.Users.Where(x => x.Email == request.UsernameOrEmail || x.Username == request.UsernameOrEmail).FirstOrDefaultAsync();
                if (user is null)
                    return RegularResponse.BadRequestError(
                        error: _localizer["UserNotFound"],
                        nextPage: Pages.login.ToString()
                    );

                if (user.Suspended)
                    return RegularResponse.BadRequestError(error: _localizer["UserSuspended"]);

                if (request.Otp != user.Otp)
                    return RegularResponse.BadRequestError(
                        error: _localizer["OtpNotMatch"]
                    );

                if (user.OtpExp < DateTime.UtcNow)
                    return RegularResponse.BadRequestError(
                        error: _localizer["OtpExpired"]
                    );

                user.Password = StorePassword(request.NewPassword, user.Salt);
                user.OtpExp = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                try
                {
                    user.Notifications.Add(new()
                    {
                        Temp = await _context.NotificationTemplates.Where(x => x.TempName == NotificationTemplates.Reset_Password.ToString()).FirstOrDefaultAsync()
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
            catch (Exception ex)
            {
                _logger.LogError("{Message}", ex.Message);

                return RegularResponse.UnknownError(_localizer);
            }
        }

        public async Task<RegularResponse> ChangePassword(ChangeKnownPasswordDto request, int user_id)
        {
            try
            {
                User? user = await _context.Users.FindAsync(user_id);

                Role role = await GetCurrentRole(user!.Id);

                if (StorePassword(request.CurrentPassword, user.Salt) != user.Password)
                    return RegularResponse.BadRequestError(
                        error: _localizer["IncorrectCurrentPassword"]
                    );

                user.Password = StorePassword(request.NewPassword, user.Salt);
                await _context.SaveChangesAsync();

                try
                {
                    user.Notifications.Add(new()
                    {
                        Temp = await _context.NotificationTemplates.Where(x => x.TempName == NotificationTemplates.Changed_Password.ToString()).FirstOrDefaultAsync()
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
            catch (Exception ex)
            {
                _logger.LogError("{Message}", ex.Message);

                return RegularResponse.UnknownError(_localizer);
            }
        }
        public async Task<RegularResponse> RequestEmailOtp(int user_id)
        {
            try
            {
                User? user = await _context.Users.FindAsync(user_id);
                if (user is null)
                    return RegularResponse.BadRequestError(
                        error: _localizer["UserNotFound"],
                        nextPage: Pages.login.ToString()
                    );

                if (user.Suspended)
                    return RegularResponse.BadRequestError(error: _localizer["UserSuspended"]);

                if (user.IsVerified)
                {
                    Role role = await GetCurrentRole(user.Id);
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
                    return RegularResponse.BadRequestError(
                        error: _localizer["emailalreadyverified"],
                        nextPage: nextPage
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
                    _logger.LogError("Sending Email Error : {Message}", ex.Message);
                    return new RegularResponse()
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        Error = _localizer["ErrorSendingEmail"],
                        NextPage = Pages.same.ToString()
                    };
                }

                return RegularResponse.Success(
                    message: _localizer["SentOtpSuccessfully"],
                    nextPage: Pages.same.ToString()
                );
            }
            catch (Exception ex)
            {
                _logger.LogError("{Message}", ex.Message);

                return RegularResponse.UnknownError(_localizer);
            }
        }
        public async Task<RegularResponse> RequestEmailOtp(string usernameOrEmail)
        {
            try
            {
                User? user = await _context.Users.Where(x => x.Email == usernameOrEmail || x.Username == usernameOrEmail).FirstOrDefaultAsync();
                if (user is null)
                    return RegularResponse.BadRequestError(
                        error: _localizer["UserNotFound"]
                    );

                if (user.Suspended)
                    return RegularResponse.BadRequestError(error: _localizer["UserSuspended"]);

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
                    _logger.LogError("Sending Email Error : {Message}", ex.Message);
                    return new RegularResponse()
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        Error = _localizer["ErrorSendingEmail"],
                        NextPage = Pages.same.ToString()
                    };
                }

                return RegularResponse.Success(
                    message: _localizer["SentOtpSuccessfully"],
                    nextPage: Pages.same.ToString()
                );
            }
            catch (Exception ex)
            {
                _logger.LogError("{Message}", ex.Message);

                return RegularResponse.UnknownError(_localizer);
            }
        }

        public async Task<RegularResponse> RedirectToHome(int user_id)
        {
            Role role = await GetCurrentRole(user_id);
            return RegularResponse.Success(
                nextPage: Pages.home.ToString() + role.ToString()
            );
        }
    }
}

