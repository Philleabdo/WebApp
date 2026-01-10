using grupp6WebApp.Data;
using grupp6WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net; // För lösenordshantering

namespace grupp6WebApp.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Visa profilen för en specifik användare (t.ex. /User/Index/1)
        public async Task<IActionResult> Index(int id)
        {
            // Vi hämtar användaren och inkluderar deras profil i samma sökning
            var profile = await _context.Profiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == id);

            if (profile == null)
            {
                return NotFound("Profilen hittades inte.");
            }

            return View(profile);
        }

        // 2. Registrera en ny användare + skapa deras profil samtidigt
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                // Hasha lösenordet innan vi sparar
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

                // Spara användaren
                _context.Users.Add(user);
                await _context.SaveChangesAsync(); // Här skapas UserId (Identity 1,1)

                // Skapa en tom profil automatiskt för den nya användaren
                var newProfile = new Profile
                {
                    UserId = user.UserId, // Koppla ihop dem!
                    Bio = "Välkommen till min profil!",
                    IsPublic = true
                };

                _context.Profiles.Add(newProfile);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", new { id = user.UserId });
            }
            return View(user);
        }

        // 3. Uppdatera profil (Kyris del)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Profile model)
        {
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.ProfileId == model.ProfileId);

            if (profile == null) return NotFound();

            // Uppdatera fälten i Profile-tabellen
            profile.Bio = model.Bio;
            profile.Skills = model.Skills;
            profile.Education = model.Education;
            profile.Experience = model.Experience;
            profile.IsPublic = model.IsPublic;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id = profile.UserId });
        }
    }
}