using System.Security.Claims;
using grupp6WebApp.Data;
using grupp6WebApp.Models.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace grupp6WebApp.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public LoginModel(ApplicationDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public LoginVm Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Rensa eventuella mellanslag i e-posten
            var email = Input.Email.Trim();

            // Hämta användaren från din egen User-tabell
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);

            // Kontrollera om användaren finns och att lösenordet (BCrypt) stämmer
            if (user == null || !BCrypt.Net.BCrypt.Verify(Input.Password, user.Password))
            {
                ErrorMessage = "Fel e-post eller lösenord.";
                return Page();
            }

            // Skapa de uppgifter (Claims) som ska sparas i din Cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                // Detta Name används av @User.Identity.Name i din _Layout
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProps = new AuthenticationProperties
            {
                IsPersistent = Input.RememberMe,
                ExpiresUtc = Input.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(7)
                    : DateTimeOffset.UtcNow.AddHours(2)
            };

            // LOGGA IN: Skapar cookien "group6.auth" i webbläsaren
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProps);

            // Redirectar till HomeController Index (din MVC-startsida)
            return RedirectToRoute(new { controller = "Home", action = "Index" });
        }
    }
}
