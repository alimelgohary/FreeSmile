using FreeSmile.Controllers;
using FreeSmile.DTOs;
using FreeSmile.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Localization;
using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using static FreeSmile.Services.Helper;

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
            _pepper = GetEnvVariable("PEPPER", true);

        }
        public async Task<ResponseDTO> AddUserAsync(UserRegisterDto userDto)
        {
            var salt = CreateSalt();
            var passEnc = Encrypt(userDto.Password, _pepper);
            var passHashed = Hash256(passEnc, salt);
            
            var otp = GenerateOtp();
            var user = new User()
            {
                Username = userDto.Username,
                Email = userDto.Email,
                Password = passHashed,
                Salt = salt,
                Phone = userDto.Phone,
                Fullname = userDto.Fullname,
                Gender = userDto.Gender,
                Bd = userDto.Birthdate,
                Otp = otp,
                OtpExp = DateTime.Now.AddMinutes(10)
            };

            await _context.AddAsync(user);
            await _context.SaveChangesAsync();
            
            
            try
            {
                SendEmail(userDto.Email, otp);
            }
            catch (Exception ex)
            {
                _logger.LogError("Sending Email Error : " + ex.Message);
            }
            return new ResponseDTO() { Id = user.Id, Error = "" };
        }

        private void SendEmail(string email, string otp)
        {
            // TODO: Send email
            throw new Exception();
            Console.WriteLine($"Sending email to {email} with otp: {otp}");
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
    }
}
