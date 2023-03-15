using System.ComponentModel.DataAnnotations;
using System.Resources;
using System.Text.RegularExpressions;

namespace FreeSmile.CustomValidations
{
    public class ValidDateAttribute : RequiredAttribute //For validation to work
    {
        public ValidDateAttribute() { }
        
        public override bool IsValid(object value)
        {
            if (value is null)
                return true;

            if (DateTime.TryParse(value.ToString(), out DateTime date))
                return true;

            return false;
        }
    }
}
