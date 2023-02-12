using System.ComponentModel.DataAnnotations;

namespace FreeSmile.CustomValidations
{
    public class MaxFileSizeAttribute : MaxLengthAttribute
    {
        private readonly int _maxFileSizeMb;
        public MaxFileSizeAttribute(int maxFileSize):base(maxFileSize)
        {
            _maxFileSizeMb = maxFileSize;
        }
        public override bool IsValid(object? value)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                if (file.Length > _maxFileSizeMb * 1024 * 1024)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
