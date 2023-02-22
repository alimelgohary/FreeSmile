using FreeSmile.Controllers;
using FreeSmile.DTOs;
using FreeSmile.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Localization;
using System;
using System.Collections;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using static FreeSmile.Services.Helper;
using static System.Net.WebRequestMethods;

namespace FreeSmile.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IStringLocalizer<UsersController> _localizer;
        private readonly FreeSmileContext _context;
        private readonly string _pepper;

        public UserService(ILogger<UsersController> logger, FreeSmileContext context, IStringLocalizer<UsersController> localizer)
        {
            _logger = logger;
            _context = context;
            _localizer = localizer;
            _pepper = MyConstants.PEPPER;

        }
        public async Task<RegularResponse> AddUserAsync(UserRegisterDto userDto, IResponseCookies cookies)
        {
            // Allow duplicate emails if user not verified yet
            _context.Users.RemoveRange(_context.Users.Where(x => x.Email == userDto.Email && x.IsVerified == false));
            await _context.SaveChangesAsync();

            // Allow duplicate phones if user not verified yet
            _context.Users.RemoveRange(_context.Users.Where(x => x.Phone == userDto.Phone && x.IsVerified == false));
            await _context.SaveChangesAsync();


            var salt = CreateSalt();
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
                Bd = userDto.Birthdate
            };

            await _context.AddAsync(user);
            await _context.SaveChangesAsync();

            return new RegularResponse() { Id = user.Id };
        }

        private void SendEmailOtp(string email, string otp)
        {
            // TODO: Send email
            //throw new NotImplementedException();
            Console.WriteLine($"Sending email to {email} with otp: {otp}");
        }

        string StorePassword(string plainText, string salt)
        {
            var passEnc = Encrypt(plainText, _pepper);
            var hash = Hash256(passEnc, salt);
            return hash;
        }

        string GenerateOtp()
        {
            int length = 6;
            int power = (int)Math.Pow(10.0, Convert.ToDouble(length));
            int lowerBound = power / 10;
            int higherBound = power - 1;
            var otp = new Random().Next(lowerBound, higherBound).ToString();
            return otp;
        }

        string Hash256(string password, string salt)
        {
            var str = password + salt;
            StringBuilder Sb = new StringBuilder();

            using (var hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(str));

                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }

        string Encrypt(string password, string key)
        {

            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] iv = new byte[16]; // Generate a random IV for added security
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(password);

            using Aes aes = Aes.Create();
            aes.Key = keyBytes;
            aes.IV = iv;
            using var encryptor = aes.CreateEncryptor();
            byte[] encryptedBytes = encryptor.TransformFinalBlock(plainTextBytes, 0, plainTextBytes.Length);
            return Convert.ToBase64String(encryptedBytes);
        }

        string Decrypt(string encryptedText, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] iv = new byte[16];
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

            using Aes aes = Aes.Create();
            aes.Key = keyBytes;
            aes.IV = iv;
            using var decryptor = aes.CreateDecryptor();
            byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        string CreateSalt()
        {
            int length = 10;
            Random random = new Random();
            var salt = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*", length)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            return salt;
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
                await _context.SaveChangesAsync();

                string nextPage = (role == AuthHelper.Role.Dentist) ? Pages.verifyDentist.ToString() : Pages.home.ToString() + role.ToString();


                return new RegularResponse()
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = _localizer["EmailVerificationSuccess"],
                    NextPage = nextPage
                };
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);

                return new RegularResponse()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Error = _localizer["UnknownError"]
                };
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
                    return new RegularResponse()
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Error = _localizer["IncorrectCreds"],
                        NextPage = Pages.same.ToString()
                    };

                AuthHelper.Role role = await GetCurrentRole(user.Id);

                TimeSpan loginTokenAge = MyConstants.LOGIN_TOKEN_AGE;

                string token = AuthHelper.GetToken(user.Id, loginTokenAge, role);
                cookies.Append(MyConstants.AUTH_COOKIE_KEY, token, new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.Strict, MaxAge = loginTokenAge });

                string nextPage = Pages.home.ToString() + role.ToString();

                if (role == AuthHelper.Role.Dentist)
                {
                    var dentist = await _context.Dentists.FindAsync(user.Id);
                    if (dentist.IsVerifiedDentist == false)
                        nextPage = Pages.verifyDentist.ToString();
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
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);

                return new RegularResponse()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Error = _localizer["UnknownError"]
                };
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

                user.Otp = GenerateOtp();
                user.OtpExp = DateTime.UtcNow + MyConstants.OTP_AGE;

                await _context.SaveChangesAsync();
                
                try
                {
                    SendEmailOtp(user.Email, user.Otp);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Sending Email Error : " + ex.Message);
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
                _logger.LogError(ex.Message);

                return new RegularResponse()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Error = _localizer["UnknownError"]
                };
            }
        }
    }
}
