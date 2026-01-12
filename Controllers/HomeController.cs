using grupp6WebApp.Models;
using grupp6WebApp.Data; // Viktigt för ApplicationDbContext
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace grupp6WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        // 1. Du måste deklarera _context här
        private readonly ApplicationDbContext _context;

        // 2. Du måste ta emot både logger och context i constructorn
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context; // Här tilldelas den så att Index kan använda den
        }

        public async Task<IActionResult> Index()
        {
            // Nu kommer denna rad att fungera!
            var latestProjects = await _context.Projects
                .OrderByDescending(p => p.CreatedDate)
                .Take(3)
                .ToListAsync();

            return View(latestProjects);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
