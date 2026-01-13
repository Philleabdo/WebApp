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
            var viewModel = new HomeIndexViewModel();

            // 1. Hämta de 3 senaste projekten (som innan)
            viewModel.LatestProjects = await _context.Projects
                .OrderByDescending(p => p.CreatedDate)
                .Take(3)
                .ToListAsync();

            // 2. Hämta 3 föreslagna användare
            // FILTER: Måste vara IsActive OCH ha en profil som är IsPublic
            viewModel.SuggestedUsers = await _context.Users
                .Include(u => u.Profile)
                .Where(u => u.IsActive && u.Profile != null && u.Profile.IsPublic)
                .Take(3)
                .ToListAsync();

            return View(viewModel);
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
