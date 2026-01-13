using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using grupp6WebApp.Data;
using grupp6WebApp.Models; // Ser till att SearchResultsViewModel hittas

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
        // Skapa din nya behållare
        var viewModel = new SearchResultsViewModel { Query = q };

        if (string.IsNullOrWhiteSpace(q))
        {
            // Returnerar en tom modell om inget sökord finns
            return View(viewModel);
        }

        var lowerQ = q.ToLower();

        // 1. Sök i Projekt
        viewModel.Projects = await _context.Projects
            .Where(p => p.Title.ToLower().Contains(lowerQ) ||
                        (p.Description != null && p.Description.ToLower().Contains(lowerQ)))
            .ToListAsync();

        // 2. Sök i Användare/Profiler
        viewModel.Users = await _context.Users
            .Include(u => u.Profile)
            .Where(u => u.IsActive && u.Profile != null && u.Profile.IsPublic)
            .Where(u => u.FirstName.ToLower().Contains(lowerQ) ||
                        u.LastName.ToLower().Contains(lowerQ) ||
                        (u.Profile.Category != null && u.Profile.Category.ToLower().Contains(lowerQ)) ||
                        (u.Profile.Skills != null && u.Profile.Skills.ToLower().Contains(lowerQ)))
            .ToListAsync();

        return View(viewModel);
    }
}