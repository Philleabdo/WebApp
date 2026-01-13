using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using grupp6WebApp.Data;
using Microsoft.EntityFrameworkCore;

namespace grupp6WebApp.Pages.Account;

public class ReactivateModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public ReactivateModel(ApplicationDbContext db)
    {
        _db = db;
    }

    // Detta är egenskapen som din HTML letade efter!
    [BindProperty]
    public int UserId { get; set; }

    // Körs när man landar på sidan
    public void OnGet(int userId)
    {
        UserId = userId;
    }

    // Körs när man klickar på den stora knappen
    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _db.Users.FindAsync(UserId);

        if (user != null)
        {
            user.IsActive = true; // Nu gör vi kontot aktivt igen!
            await _db.SaveChangesAsync();

            // Skicka tillbaka dem till login så de kan logga in på riktigt
            TempData["SuccessMessage"] = "Ditt konto är nu återaktiverat! Du kan logga in.";
            return RedirectToPage("/Account/Login");
        }

        return RedirectToRoute(new { controller = "Home", action = "Index" });
    }
}