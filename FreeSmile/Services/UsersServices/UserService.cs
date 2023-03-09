using FreeSmile.Controllers;
using FreeSmile.DTOs;
using FreeSmile.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
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


            var salt = AuthHelper.GenerateSalt();
            string storedPass = AuthHelper.StorePassword(userDto.Password, salt);

            var user = new User()
            {
                Username = userDto.Username,
                Email = userDto.Email,
                Password = storedPass,
                Salt = salt,
                Phone = userDto.Phone,
                Fullname = userDto.Fullname,
                Gender = userDto.Gender,
                Bd = userDto.Birthdate
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
                if (user is null)
                    return new RegularResponse()
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Error = _localizer["UserNotFound"],
                        NextPage = Pages.login.ToString()
                    };

                AuthHelper.Role role = await GetCurrentRole(user.Id);

                if (user.IsVerified)
                    return new RegularResponse()
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Error = _localizer["UserAlreadyVerified"],
                        NextPage = Pages.home.ToString() + role.ToString()
                    };

                if (user.Otp != otp)
                    return new RegularResponse()
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Error = _localizer["OtpNotMatch"],
                        NextPage = Pages.same.ToString()
                    };

                if (user.OtpExp < DateTime.UtcNow)
                    return new RegularResponse()
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Error = _localizer["OtpExpired"],
                        NextPage = Pages.same.ToString()
                    };

                user.IsVerified = true;
                user.OtpExp = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                string nextPage = (role == AuthHelper.Role.Dentist) ? Pages.verifyDentist.ToString() : Pages.home.ToString() + role.ToString();


                return new RegularResponse()
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = _localizer["EmailVerificationSuccess"],
                    NextPage = nextPage
                };
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
                 || AuthHelper.StorePassword(value.Password, user.Salt) != user.Password)
                    return new RegularResponse()
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Error = _localizer["IncorrectCreds"],
                        NextPage = Pages.same.ToString()
                    };

                AuthHelper.Role role = await GetCurrentRole(user.Id);

                TimeSpan loginTokenAge = MyConstants.LOGIN_TOKEN_AGE;

                string token = AuthHelper.GetToken(user.Id, loginTokenAge, role);
                cookies.Append(MyConstants.AUTH_COOKIE_KEY, token, new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.None, MaxAge = loginTokenAge, Secure = true });

                string nextPage = Pages.home.ToString() + role.ToString();

                if (role == AuthHelper.Role.Dentist)
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

                return new RegularResponse()
                {
                    StatusCode = StatusCodes.Status200OK,
                    Token = token,
                    NextPage = nextPage,
                    Message = _localizer["LoginSuccess"]
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("{Message}", ex.Message);

                return RegularResponse.UnknownError(_localizer);
            }
        }
        async Task<AuthHelper.Role> GetCurrentRole(int user_id)
        {
            AuthHelper.Role role = AuthHelper.Role.Patient;

            if (await _context.SuperAdmins.AnyAsync(x => x.SuperAdminId == user_id))
                role = AuthHelper.Role.SuperAdmin;

            if (await _context.Dentists.AnyAsync(x => x.DentistId == user_id))
                role = AuthHelper.Role.Dentist;

            if (await _context.Patients.AnyAsync(x => x.PatientId == user_id))
                role = AuthHelper.Role.Patient;

            if (await _context.Admins.AnyAsync(x => x.AdminId == user_id))
                role = AuthHelper.Role.Admin;

            return role;
        }

        public async Task<RegularResponse> RequestEmailOtp(int user_id)
        {
            try
            {
                User? user = await _context.Users.FindAsync(user_id);
                if (user is null)
                    return new RegularResponse()
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Error = _localizer["UserNotFound"],
                        NextPage = Pages.login.ToString()
                    };

                user.Otp = AuthHelper.GenerateOtp();
                user.OtpExp = DateTime.UtcNow + MyConstants.OTP_AGE;

                await _context.SaveChangesAsync();

                try
                {
                    AuthHelper.SendEmailOtp(user.Email, user.Username, user.Otp, _localizer["otpemailsubject"], _localizer["lang"]);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Sending Email Error : {Message}", ex.Message);
                    return new RegularResponse()
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        Message = _localizer["ErrorSendingEmail"],
                        NextPage = Pages.same.ToString()
                    };
                }

                return new RegularResponse()
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = _localizer["SentOtpSuccessfully"],
                    NextPage = Pages.same.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("{Message}", ex.Message);

                return RegularResponse.UnknownError(_localizer);
            }
        }
        public async Task<RegularResponse> ChangePassword(ChangeUnknownPasswordDto request)
        {
            try
            {
                User? user = await _context.Users.Where(x => x.Email == request.UsernameOrEmail || x.Username == request.UsernameOrEmail).FirstOrDefaultAsync();
                if (user is null)
                    return new RegularResponse()
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Error = _localizer["UserNotFound"],
                        NextPage = Pages.login.ToString()
                    };

                if (request.Otp != user.Otp)
                    return new RegularResponse()
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Error = _localizer["OtpNotMatch"],
                        NextPage = Pages.same.ToString()
                    };

                if (user.OtpExp < DateTime.UtcNow)
                    return new RegularResponse()
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Error = _localizer["OtpExpired"],
                        NextPage = Pages.same.ToString()
                    };

                user.Password = AuthHelper.StorePassword(request.NewPassword, user.Salt);
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

                return new RegularResponse()
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = _localizer["PasswordChangedSuccessfully"],
                    NextPage = Pages.login.ToString()
                };
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
                if (user is null)
                    return new RegularResponse()
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Error = _localizer["UserNotFound"],
                        NextPage = Pages.login.ToString()
                    };

                AuthHelper.Role role = await GetCurrentRole(user.Id);

                if (AuthHelper.StorePassword(request.CurrentPassword, user.Salt) != user.Password)
                    return new RegularResponse()
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Error = _localizer["IncorrectCurrentPassword"],
                        NextPage = Pages.same.ToString()
                    };

                user.Password = AuthHelper.StorePassword(request.NewPassword, user.Salt);
                await _context.SaveChangesAsync();

                try
                {
                    user.Notifications.Add(new() {
                        Temp = await _context.NotificationTemplates.Where(x => x.TempName == NotificationTemplates.Changed_Password.ToString()).FirstOrDefaultAsync()
                    });
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError("{Message}", ex.Message);
                }

                return new RegularResponse()
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = _localizer["PasswordChangedSuccessfully"],
                    NextPage = Pages.same.ToString()
                };
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
                    return new RegularResponse()
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Error = _localizer["UserNotFound"],
                        NextPage = Pages.same.ToString()
                    };

                user.Otp = AuthHelper.GenerateOtp();
                user.OtpExp = DateTime.UtcNow + MyConstants.OTP_AGE;

                await _context.SaveChangesAsync();

                try
                {
                    AuthHelper.SendEmailOtp(user.Email, user.Username, user.Otp, _localizer["otpemailsubject"], _localizer["lang"]);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Sending Email Error : {Message}", ex.Message);
                    return new RegularResponse()
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        Message = _localizer["ErrorSendingEmail"],
                        NextPage = Pages.same.ToString()
                    };
                }

                return new RegularResponse()
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = _localizer["SentOtpSuccessfully"],
                    NextPage = Pages.same.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("{Message}", ex.Message);

                return RegularResponse.UnknownError(_localizer);
            }
        }

        public async Task<bool> IsNotSuspended(int id)
        {
            User? user = await _context.Users.FindAsync(id);
            if (user is null)
                return true;
            if (user.Suspended)
                return false;
            return true;
        }
        public async Task<bool> IsNotSuspended(string usernameOrEmail)
        {
            User? user = await _context.Users.Where(x=> x.Username == usernameOrEmail || x.Email == usernameOrEmail).FirstOrDefaultAsync();
            if (user is null)
                return true;
            if (user.Suspended)
                return false;
            return true;
        }
        public async Task<bool> IsVerifiedEmail(int id)
        {
            User? user = await _context.Users.FindAsync(id);
            if (user is null)
                return true;
            if (user.IsVerified)
                return true;
            return false;
        }
        public async Task<bool> IsVerifiedEmail(string usernameOrEmail)
        {
            User? user = await _context.Users.Where(x => x.Username == usernameOrEmail || x.Email == usernameOrEmail).FirstOrDefaultAsync();
            if (user is null)
                return true;
            if (user.IsVerified)
                return true;
            return false;
        }

        public async Task<bool> InitialChecks(string usernameOrEmail)
        {
            return await IsVerifiedEmail(usernameOrEmail)
                && await IsNotSuspended(usernameOrEmail);
        }

        public async Task<bool> InitialChecks(int id)
        {
            return await IsVerifiedEmail(id)
                && await IsNotSuspended(id);
        }
    }
}

