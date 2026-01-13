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

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var project = await _context.Projects
            .Include(p => p.User) // Skaparen
            .Include(p => p.UsersWhoDisplay) // Alla som bockat i projektet (Many-to-Many)
            .FirstOrDefaultAsync(p => p.ProjectId == id);

        if (project == null)
        {
            return NotFound();
        }

        return View(project);
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

    // --- NYA EDIT-METODER ---

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null) return NotFound();

        // Endast skaparen får redigera
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (project.UserId != currentUserId) return Forbid();

        return View(project);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Project project)
    {
        if (id != project.ProjectId) return NotFound();

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // Hämta befintligt projekt för att kolla ägarskap (AsNoTracking för att undvika krock i context)
        var existingProject = await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.ProjectId == id);
        if (existingProject == null) return NotFound();
        if (existingProject.UserId != currentUserId) return Forbid();

        if (ModelState.IsValid)
        {
            try
            {
                // Säkerställ att UserId och CreatedDate inte ändras av misstag
                project.UserId = currentUserId;
                _context.Update(project);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Projektet har uppdaterats!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(project.ProjectId)) return NotFound();
                else throw;
            }
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

    private bool ProjectExists(int id)
    {
        return _context.Projects.Any(e => e.ProjectId == id);
    }
}