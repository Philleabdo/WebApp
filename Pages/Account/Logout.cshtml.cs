using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace grupp6WebApp.Pages.Account
{
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync()
        {
            // ÄNDRA HÄR: Använd standard-schemat istället för "CookieAuth"
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Skicka användaren till startsidan efter utloggning
            return RedirectToRoute(new { controller = "Home", action = "Index" });
        }
    }
}
