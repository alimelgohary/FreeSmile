using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;

namespace FreeSmile.CustomValidations
{
    public class CommaArrayIntAttribute : ValidationAttribute
    {
        public new string ErrorMessage { get; set; } = "InvalidValue";
        public CommaArrayIntAttribute() { }

        public override bool IsValid(object? value)
        {
            if (value is null)
                return true;

            string[] values = value.ToString()!.Split(',');
            foreach (var item in values)
            {
                if (!int.TryParse(item, out _))
                    return false;
            }

            return true;
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
