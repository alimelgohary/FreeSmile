using System.ComponentModel.DataAnnotations;

namespace FreeSmile.CustomValidations
{
    /// <summary>
    /// Used on IFormFileCollection, IFormFile to validate the extension of each file in the collection
    /// </summary>
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
                    return false;
            }
            else
            {
                var collection = value as IFormFileCollection;
                if(collection != null)
                {
                    foreach (var item in collection)
                    {
                        var extension = Path.GetExtension(item?.FileName);
                        if (!_extensions.Contains(extension?.ToLower()))
                            return false;
                    }
                }
            }

            return true;
        }
    }
}
