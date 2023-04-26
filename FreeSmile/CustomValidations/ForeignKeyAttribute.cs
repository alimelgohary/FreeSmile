using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace FreeSmile.CustomValidations
{
    public class ForeignKeyAttribute : ValidationAttribute
    {
        public string className { get; set; }
        public string colName { get; set; }
        bool bypassZero { get; set; }
        public new string ErrorMessage { get; set; } = "InvalidChoice";
        public ForeignKeyAttribute(string className, string colName, bool bypassZero = false)
        {
            this.className = className;
            this.colName = colName;
            this.bypassZero = bypassZero;
        }

        public override bool IsValid(object? value)
        {
            if (value == null)
                return true;

            if (bypassZero && (int)value == 0)
                return true;
            
            if (Exsists(className, colName, (int)value))
                return true;

            return false;
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if(IsValid(value))
                return ValidationResult.Success;

            var _localizer = validationContext.GetService(typeof(IStringLocalizer<ValidationAttribute>)) as IStringLocalizer<ValidationAttribute>;
            var error = _localizer![ErrorMessage, _localizer[validationContext.DisplayName]];
            return new ValidationResult(error);
        }
        public static bool Exsists(string className, string colName, int value)
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

            if (result.ToString() == 1.ToString())
                return true;
            else
                return false;
        }

        private static string Plural(string noun)
        {
            if(noun.EndsWith('y'))
            {
                return noun.Substring(0, noun.Length - 1) + "ies";
            }
            return noun + 's';
        }
    }
}

