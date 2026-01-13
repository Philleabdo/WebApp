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

            var email = Input.Email.Trim();

            // 1. Hämta användaren
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);

            // 2. Kontrollera om användaren finns och att lösenordet stämmer
            if (user == null || !BCrypt.Net.BCrypt.Verify(Input.Password, user.Password))
            {
                ErrorMessage = "Fel e-post eller lösenord.";
                return Page();
            }

            // --- 3. ÄNDRAT HÄR: Skicka till Reactivate-vyn istället för att bara logga in ---
            if (!user.IsActive)
            {
                // Vi skickar användaren till den nya sidan och skickar med deras ID i adressfältet
                return RedirectToPage("/Account/Reactivate", new { userId = user.UserId });
            }
            // -------------------------------------------------------------------------------

            // 4. Skapa Claims för inloggningen (om de är aktiva)
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
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

            // 5. Logga in
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProps);

            return RedirectToRoute(new { controller = "Home", action = "Index" });
        }
    }
}
