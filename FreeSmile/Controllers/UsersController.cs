using FreeSmile.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
        [HttpPost("RegisterPatient")]
        public void Register([FromBody] BlaBla value, string id)
        {
            Console.WriteLine($"Post {value.Name} {value.Id} {id}");
        }
        [HttpGet]
        public IEnumerable<string> Get()
        {
            _context.CaseTypes.ToList().ForEach(x => Console.WriteLine(x.NameEn + x.NameAr));
            return new string[] { "value1", "value2" };
        }

        [HttpGet("lang")]
        public string Get(string id)
        {
            return $"GEtvalue {string.Join(", ", _context.ArticleCats.Select(x => new { lang = x.Lang(id)}).ToList())}";
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
            public int Id { get; set; }
            public string Name { get; set; }

        }
    }
}
