using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FreeSmile.DTOs.Query
{
    public class SizeDto
    {
        [DisplayName(nameof(Size))]
        [Range(0, 64, ErrorMessage = "boundvalue")]
        public int Size { get; set; } = 10;
    }
}