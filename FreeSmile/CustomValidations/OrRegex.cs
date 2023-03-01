using System.ComponentModel.DataAnnotations;
using System.Resources;
using System.Text.RegularExpressions;

namespace FreeSmile.CustomValidations
{
    public class OrRegexAttribute : RequiredAttribute //For validation to work
    {
        public string[] Regexes{ get; set; }
        public OrRegexAttribute(params string[] regexes)
        {
            this.Regexes = regexes;
        }
        
        public override bool IsValid(object value)
        {
            string? valueStr = value as string;
            if (valueStr is null)
                return true;
            
            foreach (var regex in Regexes)
            {
                Regex re = new Regex(regex);
                
                if (re.IsMatch(valueStr!))
                    return true;
            }
            return false;
            
        }

    }
}
