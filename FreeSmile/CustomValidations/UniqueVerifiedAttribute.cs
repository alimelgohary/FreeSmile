using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Localization;

namespace FreeSmile.CustomValidations
{
    public class UniqueVerifiedAttribute : ValidationAttribute
    {
        public string colName { get; set; }
        public new string ErrorMessage { get; set; } = "Unique";

        public UniqueVerifiedAttribute(string className, string colName)
        {
            this.colName = colName;
        }
        public override bool IsValid(object? value)
        {
            if (value == null)
                return true;
            
            if (IsUnique(colName, value.ToString()))
                return true;
            
            return false;
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (IsValid(value))
                return ValidationResult.Success;

            var _localizer = validationContext.GetService(typeof(IStringLocalizer<ValidationAttribute>)) as IStringLocalizer<ValidationAttribute>;
            var error = _localizer![ErrorMessage, _localizer[validationContext.DisplayName]];
            return new ValidationResult(error);
        }
        public static bool IsUnique(string colName, string value)
        {
            var query = $"SELECT COUNT(*) FROM Users WHERE {colName}=@{colName} AND isVerified=1";
            using var context = new FreeSmileContext();
            using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            command.Parameters.Add(new SqlParameter($"@{colName}", value));
            context.Database.OpenConnection();

            var result = command.ExecuteScalar();
            
            if (result.ToString() == 0.ToString())
                return true;
            else
                return false;
        }

    }
}

