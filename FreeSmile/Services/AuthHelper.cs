using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FreeSmile.Services
{
    public class AuthHelper
    {
        private static string GenerateToken(ClaimsIdentity claims, TimeSpan tokenAge)
        {
            JwtSecurityTokenHandler tokenHandler = new();
            byte[] key = Encoding.ASCII.GetBytes(MyConstants.JWT_SECRET);
            SecurityTokenDescriptor tokenDescriptor = new()
            {

                Subject = claims,
                Expires = DateTime.UtcNow + tokenAge,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
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

        private static string TokenPatient(int sub, TimeSpan age)
        {
            return GenerateToken(PATIENT_CLAIMS(sub), age);
        }
        private static string TokenAdmin(int sub, TimeSpan age)
        {
            return GenerateToken(ADMIN_CLAIMS(sub), age);
        }
        private static string TokenDentist(int sub, TimeSpan age)
        {
            return GenerateToken(DENTIST_CLAIMS(sub), age);
        }
        private static string TokenSuperAdmin(int sub, TimeSpan age)
        {
            return GenerateToken(SUPER_ADMIN_CLAIMS(sub), age);
        }
        public static string GetToken(int user_id, TimeSpan tokenAge, Role role)
        {
            string token;
            switch (role)
            {
                case Role.Admin:
                    token = TokenAdmin(user_id, tokenAge);
                    break;
                case Role.Dentist:
                    token = TokenDentist(user_id, tokenAge);
                    break;
                case Role.SuperAdmin:
                    token = TokenSuperAdmin(user_id, tokenAge);
                    break;
                default:
                    token = TokenPatient(user_id, tokenAge);
                    break;
            }
            return token;
        }
    }
}
