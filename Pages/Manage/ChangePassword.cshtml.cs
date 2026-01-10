using System.Security.Claims;
using BCrypt.Net;
using grupp6WebApp.Data;
using grupp6WebApp.Models.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace grupp6WebApp.Pages.Manage;

[Authorize]
public class ChangePasswordModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public ChangePasswordModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public ChangePasswordVm Input { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        // Hämta UserId från cookie-claim
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
            return RedirectToPage("/Account/Login");

        // Hämta user från DB
        var user = await _db.Users.SingleOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
            return RedirectToPage("/Account/Login");

        // Kontrollera nuvarande lösenord
        if (!BCrypt.Net.BCrypt.Verify(Input.CurrentPassword, user.Password))
        {
            ErrorMessage = "Nuvarande lösenord är fel.";
            return Page();
        }

        // Spara nytt lösenord (hash)
        user.Password = BCrypt.Net.BCrypt.HashPassword(Input.NewPassword);
        await _db.SaveChangesAsync();

        SuccessMessage = "Lösenordet är uppdaterat!";
        return Page();
    }
}
