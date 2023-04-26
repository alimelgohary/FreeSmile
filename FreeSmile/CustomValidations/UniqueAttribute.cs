using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Localization;

namespace FreeSmile.CustomValidations
{
    public class UniqueAttribute : ValidationAttribute
    {
        public string className { get; set; }
        public string colName { get; set; }
        public new string ErrorMessage { get; set; } = "Unique";

        public UniqueAttribute(string className, string colName)
        {
            this.className = className;
            this.colName = colName;
        }
        public override bool IsValid(object? value)
        {
            if (value == null)
                return true;
            
            if (IsUnique(className, colName, value.ToString()))
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
        public static bool IsUnique(string className, string colName, string value)
        {
            var tableName = Plural(className);
            var query = $"SELECT COUNT(*) FROM {tableName} WHERE {colName}=@{colName}";
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
        static string Plural(string noun)
        {
            if (noun.EndsWith('y'))
            {
                return noun.Substring(0, noun.Length - 1) + "ies";
            }
            return noun + 's';
        }
    }
}

