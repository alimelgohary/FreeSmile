using System.Security.Claims;

namespace FreeSmile.Services
{
    public class AuthHelper
    {
        public enum Role
        {
            Admin,
            Patient,
            Dentist,
            SuperAdmin
        }
        public static List<Claim> COMMON_CLAIMS(int sub)
        {
            return new()
            {
                new(ClaimTypes.NameIdentifier, sub.ToString()),
            };
        }
        public static ClaimsIdentity PATIENT_CLAIMS(int sub)
        {
            return new(
                COMMON_CLAIMS(sub).Concat(new Claim[]
                    {
                        new (ClaimTypes.Role, Role.Patient.ToString()),
                    })
                );
        }
        public static ClaimsIdentity ADMIN_CLAIMS(int sub)
        {
            return new(
                COMMON_CLAIMS(sub).Concat(new Claim[]
                    {
                        new (ClaimTypes.Role, Role.Admin.ToString())
                    })
                );
        }
        public static ClaimsIdentity DENTIST_CLAIMS(int sub)
        {
            return new(
                COMMON_CLAIMS(sub).Concat(new Claim[]
                    {
                        new (ClaimTypes.Role, Role.Dentist.ToString())
                    })
                );
        }
        public static ClaimsIdentity SUPER_ADMIN_CLAIMS(int sub)
        {
            return new(
                COMMON_CLAIMS(sub).Concat(new Claim[]
                    {
                        new (ClaimTypes.Role, Role.SuperAdmin.ToString())
                    })
                );
        }

        public static string TokenPatient(int sub, TimeSpan age)
        {
            return Helper.GenerateToken(PATIENT_CLAIMS(sub), age);
        }
        public static string TokenAdmin(int sub, TimeSpan age)
        {
            return Helper.GenerateToken(ADMIN_CLAIMS(sub), age);
        }
        public static string TokenDentist(int sub, TimeSpan age)
        {
            return Helper.GenerateToken(DENTIST_CLAIMS(sub), age);
        }
    }
}
