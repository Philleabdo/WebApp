using grupp6WebApp.Data;
using grupp6WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;

namespace grupp6WebApp.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        public IActionResult TestAdd()
        {
            try
            {
                // Det lösenord vi vill spara
                string clearTextPassword = "DittLösenord123";

                // Här skapas hashen (en lång oläslig sträng)
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(clearTextPassword);

                var testUser = new User
                {
                    FirstName = "Philip",
                    LastName = "Hash-Test",
                    Email = "philip@secure.com",
                    Address = "Säkerhetsgatan 1",
                    Password = hashedPassword // Vi sparar hashen, INTE klartexten!
                };

                _context.Users.Add(testUser);
                _context.SaveChanges();

                return Content($"Succé! Användaren har sparats. Lösenordet i databasen ser nu ut såhär: {hashedPassword}");
            }
            catch (Exception ex)
            {
                return Content($"Något gick fel: {ex.Message}");
            }
        }
    }
}