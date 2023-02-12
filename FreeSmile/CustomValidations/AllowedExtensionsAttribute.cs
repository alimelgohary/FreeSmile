using System.ComponentModel.DataAnnotations;

namespace FreeSmile.CustomValidations
{
    public class AllowedExtensionsAttribute : RequiredAttribute
    {
        private readonly string[] _extensions;
        public AllowedExtensionsAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        public override bool IsValid(object value)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                if (!_extensions.Contains(extension.ToLower()))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
