using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;

namespace FreeSmile.CustomValidations
{
    public class ValidDateAttribute : ValidationAttribute
    {
        public new string ErrorMessage { get; set; } = "InvalidValue";
        public ValidDateAttribute() { }
        
        public override bool IsValid(object? value)
        {
            if (value is null)
                return true;

            if (DateTime.TryParse(value.ToString(), out DateTime date))
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
    }
}
