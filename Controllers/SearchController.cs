using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using grupp6WebApp.Data;

namespace grupp6WebApp.Controllers;

public class SearchController : Controller
{
    private readonly ApplicationDbContext _context;

    public SearchController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Query = q; // Sparar sökordet för att visa i vyn

        if (string.IsNullOrWhiteSpace(q))
        {
            return View(new List<grupp6WebApp.Models.Project>());
        }

        var results = await _context.Projects
            .Where(p => p.Title.Contains(q) || p.Description.Contains(q))
            .ToListAsync();

        return View(results);
    }
}