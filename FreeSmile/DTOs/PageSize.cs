using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FreeSmile.DTOs
{
    public class PageSize
    {
        [DisplayName(nameof(Page))]
        [Range(0, int.MaxValue, ErrorMessage = "boundvalue")]
        public int Page { get; set; } = 1;

        [DisplayName(nameof(Size))]
        [Range(0, 64, ErrorMessage = "boundvalue")]
        public int Size { get; set; } = 10;
    }
}