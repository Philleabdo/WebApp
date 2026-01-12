using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using grupp6WebApp.Data;
using grupp6WebApp.Models;

namespace grupp6WebApp.Controllers;

public class ProjectController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProjectController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Listar alla projekt
    public async Task<IActionResult> Index()
    {
        var projects = await _context.Projects.ToListAsync();
        return View(projects); // Skickar List<Project> direkt
    }

    // Detaljer för ett projekt
    public async Task<IActionResult> Details(int id)
    {
        var project = await _context.Projects
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.ProjectId == id);

        if (project == null) return NotFound();

        return View(project); // Skickar Project-objektet direkt
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Project project)
    {
        if (ModelState.IsValid)
        {
            project.CreatedDate = DateTime.Now;
            // För teständamål sätter vi UserId till 1 (se till att User med ID 1 finns i DB)
            project.UserId = 1;

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(project);
    }

    // GET: Project/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null) return NotFound();
        return View(project);
    }

    // POST: Project/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Project project)
    {
        if (id != project.ProjectId) return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(project);
    }
}