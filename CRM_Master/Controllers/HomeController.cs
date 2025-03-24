using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace CRM_Master.Controllers
{
    [ApiController]
    [Route("api/home")]
    public class HomeController : Controller  // <-- CHANGÉ de ControllerBase à Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet("message")]
        public IActionResult GetMessage()
        {
            return View("Message"); // Fonctionnera maintenant
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            return Ok(new { message = "Page de login" });
        }

        [HttpGet("menu")]
        public IActionResult Menu()
        {
            return Ok(new { message = "Page menu" });
        }
    }
}
