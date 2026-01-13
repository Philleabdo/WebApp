using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using grupp6WebApp.Data;
using grupp6WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace grupp6WebApp.Controllers;

[Authorize]
public class ProjectController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProjectController(ApplicationDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        // Inkluderar ägaren för att kunna visa "Skapad av"
        var projects = await _context.Projects.Include(p => p.User).ToListAsync();
        return View(projects);
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Project project)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr != null)
        {
            project.UserId = int.Parse(userIdStr);
            project.CreatedDate = DateTime.Now;
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(project);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        var currentUserId = int.Parse(userIdClaim);
        var project = await _context.Projects.FindAsync(id);

        if (project == null) return NotFound();

        // SÄKERHETSKOLL: Endast den ursprungliga skaparen (UserId) får radera permanent
        if (project.UserId != currentUserId)
        {
            TempData["ErrorMessage"] = "Du kan bara radera projekt som du själv har skapat.";
            return RedirectToAction(nameof(Index));
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Projektet raderades permanent från systemet.";

        return RedirectToAction(nameof(Index));
    }
}