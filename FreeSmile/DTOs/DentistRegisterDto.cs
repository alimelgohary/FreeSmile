using FreeSmile.CustomValidations;
using FreeSmile.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FreeSmile.DTOs
{
    public class DentistRegisterDto: UserRegisterDto
    {
        public int CurrentDegree { get; set; }
        public int CurrentUniversity { get; set; }
    }
}
