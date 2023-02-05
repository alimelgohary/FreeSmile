using FreeSmile.Models;
using Microsoft.AspNetCore.Mvc;

namespace FreeSmile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly FreeSmileContext _context;
        public UsersController(ILogger<UsersController> logger, FreeSmileContext context)
        {
            _logger = logger;
            _context = context;
        }
        [HttpPost("Register")]
        public void Register([FromBody] User value)
        {
            Console.WriteLine($"Post {value}");
        }
        [HttpGet]
        public IEnumerable<string> Get()
        {
            _context.CaseTypes.ToList().ForEach(x => Console.WriteLine(x.NameEn + x.NameAr));
            return new string[] { "value1", "value2" };
        }

        [HttpGet("{id}")]
        public string Get(int id)
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
    }
}
