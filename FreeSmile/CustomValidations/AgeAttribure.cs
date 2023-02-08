using System.ComponentModel.DataAnnotations;
using System.Resources;
namespace FreeSmile.CustomValidations
{
    public class AgeAttribute : StringLengthAttribute //For validation to work
    {
        public int RequiredAgeMin { get; set; }
        public int RequiredAgeMax { get; set; }
        public AgeAttribute(int requiredAgeMin, int requiredAgeMax) : base(requiredAgeMax)
        {
            MinimumLength = requiredAgeMin;

            RequiredAgeMax = requiredAgeMax;
            RequiredAgeMin = requiredAgeMin;
        }
        
        public override bool IsValid(object value)
        {
            if (value is null)
                return true;
            
            DateTime dt = (DateTime)value;
            var spanMin = TimeSpan.FromDays(365 * RequiredAgeMin);
            var spanMax= TimeSpan.FromDays(365 * RequiredAgeMax);
            var objAge = (DateTime.Now - dt);
            if (objAge >= spanMin && objAge <= spanMax)
                return true;

            return false;
            
        }

    }
}
