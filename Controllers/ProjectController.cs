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
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var project = await _context.Projects.FindAsync(id);

        if (project == null) return NotFound();

        // SÄKERHETSKOLL: Endast skaparen får radera projektet permanent från systemet
        if (project.UserId != currentUserId)
        {
            return Forbid();
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Projektet raderades permanent.";

        return RedirectToAction(nameof(Index));
    }
}