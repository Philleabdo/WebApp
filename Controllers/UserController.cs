using grupp6WebApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace grupp6WebApp.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly ApplicationDbContext _db;

    public UserController(ApplicationDbContext db)
    {
        _db = db;
    }

    // Exempel: visa användarens profil (kopplad 1-1)
    public async Task<IActionResult> Profile()
    {
        // UserId från cookie-claim
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return RedirectToPage("/Account/Login");

        var user = await _db.Users
            .Include(u => u.Profile)
            .SingleOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            return NotFound();

        return View(user); // View kan ta User som model
    }
}
