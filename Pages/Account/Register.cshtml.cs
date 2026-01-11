using BCrypt.Net;
using grupp6WebApp.Models;
using grupp6WebApp.Data;
using grupp6WebApp.Models;
using grupp6WebApp.Models.ViewModels.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace grupp6WebApp.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public RegisterModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public RegisterVm Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        // Visar sidan
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        // 1. Kontrollera om e-post redan finns
        var emailExists = await _db.Users.AnyAsync(u => u.Email == Input.Email);
        if (emailExists)
        {
            ErrorMessage = "E-postadressen används redan.";
            return Page();
        }

        // 2. Skapa user och hasha lösenord
        var user = new User
        {
            FirstName = Input.FirstName,
            LastName = Input.LastName,
            Address = Input.Address,
            Phone = Input.Phone,
            Email = Input.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(Input.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // 3. Skapa tom profil (pga 1–1 User <-> Profile)
        var profile = new Profile
        {
            UserId = user.UserId
        };

        _db.Profiles.Add(profile);
        await _db.SaveChangesAsync();

        // 4. Skicka till login
        return RedirectToPage("/Account/Login");
    }
}
