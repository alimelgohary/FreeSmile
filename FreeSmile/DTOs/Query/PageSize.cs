using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FreeSmile.DTOs.Query
{
    public class PageSize : SizeDto
    {
        [DisplayName(nameof(Page))]
        [Range(0, int.MaxValue, ErrorMessage = "boundvalue")]
        public int Page { get; set; } = 1;
    }
}