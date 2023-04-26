using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FreeSmile.CustomValidations
{
    public class OrRegexAttribute : ValidationAttribute
    {
        public string[] Regexes{ get; set; }
        public new string ErrorMessage { get; set; } = "InvalidValue";
        public OrRegexAttribute(params string[] regexes)
        {
            this.Regexes = regexes;
        }
        
        public override bool IsValid(object? value)
        {
            string? valueStr = value as string;
            if (valueStr is null)
                return true;
            
            foreach (var regex in Regexes)
            {
                Regex re = new(regex);
                
                if (re.IsMatch(valueStr!))
                    return true;
            }
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
    }
}
