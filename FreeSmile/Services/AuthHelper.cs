using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using System.Net.Mail;
using System.Net;
using System.Net.Mime;

namespace FreeSmile.Services
{
    public class AuthHelper
    {
        private static string? JWT_SECRET { get; } = Helper.GetEnvVariable("_Jwt_Secret", false);
        
        private static string PEPPER { get; } = Helper.GetEnvVariable("_PEPPER", true)!;
        private static string? FREESMILE_GMAIL_PASSWORD { get; } = Helper.GetEnvVariable("_FreeSmileGmailPass", false);
        private static string? FREESMILE_GMAIL { get; } = Helper.GetEnvVariable("_FreeSmileGmail", false);
        private static readonly string SmtpServer = "smtp.gmail.com";
        private static readonly int SmtpPort = 587;

        public static TokenValidationParameters tokenValidationParameters = new()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(JWT_SECRET)),
            ValidateIssuer = false,
            ValidateAudience = false
        };

        public static string StorePassword(string plainText, string salt)
        {
            var passEnc = Encrypt(plainText, PEPPER);
            var hash = Hash256(passEnc, salt);
            return hash;
        }
        
        static string Hash256(string password, string salt)
        {
            var str = password + salt;
            StringBuilder Sb = new();

            using (var hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(str));

                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }

        static string Encrypt(string password, string key)
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

        static string Decrypt(string encryptedText, string key)
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
        
        public enum Role
        {
            Admin,
            Patient,
            Dentist,
            SuperAdmin
        }
        
        private static List<Claim> COMMON_CLAIMS(int sub)
        {
            return new()
            {
                new(ClaimTypes.NameIdentifier, sub.ToString()),
            };
        }
        
        private static ClaimsIdentity PATIENT_CLAIMS(int sub)
        {
            return new(
                COMMON_CLAIMS(sub).Concat(new Claim[]
                    {
                        new (ClaimTypes.Role, Role.Patient.ToString()),
                    })
                );
        }
        
        private static ClaimsIdentity ADMIN_CLAIMS(int sub)
        {
            return new(
                COMMON_CLAIMS(sub).Concat(new Claim[]
                    {
                        new (ClaimTypes.Role, Role.Admin.ToString())
                    })
                );
        }
        
        private static ClaimsIdentity DENTIST_CLAIMS(int sub)
        {
            return new(
                COMMON_CLAIMS(sub).Concat(new Claim[]
                    {
                        new (ClaimTypes.Role, Role.Dentist.ToString())
                    })
                );
        }
        
        private static ClaimsIdentity SUPER_ADMIN_CLAIMS(int sub)
        {
            return new(
                COMMON_CLAIMS(sub).Concat(new Claim[]
                    {
                        new (ClaimTypes.Role, Role.SuperAdmin.ToString())
                    })
                );
        }

        public static string GetToken(int user_id, TimeSpan tokenAge, Role role)
        {
            string token = role switch
            {
                Role.Admin => GenerateToken(ADMIN_CLAIMS(user_id), tokenAge),
                Role.Dentist => GenerateToken(DENTIST_CLAIMS(user_id), tokenAge),
                Role.SuperAdmin => GenerateToken(SUPER_ADMIN_CLAIMS(user_id), tokenAge),
                _ => GenerateToken(PATIENT_CLAIMS(user_id), tokenAge),
            };
            return token;
        }

        private static string GenerateToken(ClaimsIdentity claims, TimeSpan tokenAge)
        {
            JwtSecurityTokenHandler tokenHandler = new();
            byte[] key = Encoding.ASCII.GetBytes(JWT_SECRET);
            SecurityTokenDescriptor tokenDescriptor = new()
            {

                Subject = claims,
                Expires = DateTime.UtcNow + tokenAge,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static string GenerateOtp()
        {
            int length = 6;
            int power = (int)Math.Pow(10.0, Convert.ToDouble(length));
            int lowerBound = power / 10;
            int higherBound = power - 1;
            var otp = new Random().Next(lowerBound, higherBound).ToString();
            return otp;
        }

        public static string GenerateSalt()
        {
            int length = 10;
            Random random = new();
            var salt = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*", length)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            return salt;
        }

        public static void SendEmailOtp(string receiverEmail, string username, string otp, string subject, string lang)
        {
            var logoResource = new LinkedResource(@"EmailTemplates\logo.png", MediaTypeNames.Image.Jpeg);
            string body = string.Format(File.ReadAllText(@$"EmailTemplates\{lang}\otpemail.html"), username, MyConstants.OTP_AGE.Minutes, otp, $"cid:{logoResource.ContentId}");
            var alternateView = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
            alternateView.LinkedResources.Add(logoResource);
            MailMessage message = new()
            {
                From = new MailAddress(FREESMILE_GMAIL, "Free Smile"),
                To = { new MailAddress(receiverEmail) },
                Subject = subject,
                AlternateViews = { alternateView },
                IsBodyHtml = true
            };
            SmtpClient client = new(SmtpServer, SmtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(FREESMILE_GMAIL, FREESMILE_GMAIL_PASSWORD)
            };
            client.Send(message);
        }
    }
}
