using System.ComponentModel.DataAnnotations;

namespace FreeSmile.CustomValidations
{
    /// <summary>
    /// Used on IFormFileCollection, IFormFile to validate the size of each file in the collection
    /// </summary>
    public class MaxFileSizeAttribute : MaxLengthAttribute
    {
        private readonly int _maxFileSizeMb;
        public MaxFileSizeAttribute(int maxFileSize) : base(maxFileSize)
        {
            _maxFileSizeMb = maxFileSize;
        }
        public override bool IsValid(object? value)
        {
            if (value == null)
                return true;
            
            var file = value as IFormFile;
            if (file != null)
            {
                if (file.Length > _maxFileSizeMb * 1024 * 1024)
                    return false;
            }
            else
            {
                var collection = value as IFormFileCollection;
                if(collection != null)
                {
                    foreach (var image in collection)
                    {
                        if (image?.Length > _maxFileSizeMb * 1024 * 1024)
                            return false;
                    }
                }
            }
            return true;
        }
    }
}
