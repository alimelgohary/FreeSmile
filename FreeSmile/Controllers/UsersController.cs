using FreeSmile.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using FreeSmile.DTOs;

namespace FreeSmile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IStringLocalizer<UsersController> _localizer;
        private readonly FreeSmileContext _context;
        public UsersController(ILogger<UsersController> logger, FreeSmileContext context, IStringLocalizer<UsersController> localizer)
        {
            _logger = logger;
            _context = context;
            _localizer = localizer;
        }
        [HttpPost("RegisterPatient")]
        public IActionResult Register(UserRegisterDto value)
        {
            Console.WriteLine($"{value.Username}, {value.Gender}, {value.Email}");
            //Console.WriteLine($"Post {value.Name} {value.Id}");
            return Ok(value);
        }
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { _localizer["balbal", "Ali"], _localizer["home"], _localizer["privacy"] };
        }

        [HttpGet("lang")]
        public string Get(string id)
        {
            return $"GEtvalue {id}";
        }

        [HttpPost]
        public void Post([FromBody] string value)
        {
            Console.WriteLine($"Post {value}");
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
            Console.WriteLine($"Put {id} with {value}");
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            Console.WriteLine("Deleted" + id);
        }

        public class BlaBla
        {
            [DisplayName("iD")]
            [Required(ErrorMessage = "required")]
            public int? Id { get; set; }
            [DisplayName("name")] // useful for printing property **localized** name **in** error 
            [Required(ErrorMessage = "required")]
            public string Name { get; set; }
        }

    }
}
