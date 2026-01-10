using grupp6WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace grupp6WebApp.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly grupp6WebApp.Data.ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public UserController(grupp6WebApp.Data.ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.IdentityUserId == userId);

        if (profile is null)
        {
            profile = new Profile
            {
                IdentityUserId = userId,
                Email = User.Identity?.Name
            };

            _context.Profiles.Add(profile);
            await _context.SaveChangesAsync();
        }

        return View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Profile model)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.IdentityUserId == userId);
        if (profile is null)
            return NotFound();

        profile.FirstName = model.FirstName;
        profile.LastName = model.LastName;
        profile.Email = model.Email;
        profile.Address = model.Address;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
