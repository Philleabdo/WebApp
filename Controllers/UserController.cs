using grupp6WebApp.Data;
using grupp6WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Xml.Linq; 
using System.Text;     

namespace grupp6WebApp.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly ApplicationDbContext _db;
    private const string DefaultProfilePic = "/images/FolioAccountPofile.png";

    public UserController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Profile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId)) return RedirectToAction("Login", "Account");

        var user = await _db.Users
            .Include(u => u.Profile)
            .Include(u => u.Projects)
            .SingleOrDefaultAsync(u => u.UserId == userId);

        if (user == null) return NotFound();

        return View(MapToViewModel(user));
    }

    [HttpGet]
    public async Task<IActionResult> DownloadXml()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId)) return RedirectToAction("Login", "Account");

        var user = await _db.Users
            .Include(u => u.Profile)
            .Include(u => u.Projects)
            .SingleOrDefaultAsync(u => u.UserId == userId);

        if (user == null) return NotFound();

        // Skapa XML-struktur
        var xmlDoc = new XDocument(
            new XElement("CV",
                new XElement("Användare",
                    new XElement("Namn", $"{user.FirstName} {user.LastName}"),
                    new XElement("Epost", user.Email),
                    new XElement("Adress", user.Address)
                ),
                new XElement("Profil",
                    new XElement("Bio", user.Profile?.Bio ?? ""),
                    new XElement("Kompetenser", user.Profile?.Skills ?? ""),
                    new XElement("Utbildning", user.Profile?.Education ?? ""),
                    new XElement("Erfarenhet", user.Profile?.Experience ?? "")
                ),
                new XElement("Projekt",
                    user.Projects.Select(p => new XElement("ProjektInfo",
                        new XElement("Titel", p.Title),
                        new XElement("Beskrivning", p.Description ?? "")
                    ))
                )
            )
        );

        var bytes = Encoding.UTF8.GetBytes(xmlDoc.ToString());
        return File(bytes, "application/xml", $"CV_{user.LastName}.xml");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateAccount()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            var user = await _db.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsActive = false;
                await _db.SaveChangesAsync();

                // Här kan du lägga till utloggning om du använder Identity
                // await _signInManager.SignOutAsync(); 

                return RedirectToAction("Index", "Home");
            }
        }
        return RedirectToAction("Profile");
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId)) return RedirectToAction("Login", "Account");

        var user = await _db.Users
            .Include(u => u.Profile)
            .Include(u => u.Projects)
            .SingleOrDefaultAsync(u => u.UserId == userId);

        if (user == null) return NotFound();

        ViewBag.AllProjects = await _db.Projects.ToListAsync();
        return View(MapToViewModel(user));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(CVViewModel model, IFormFile? profileImage, int[] selectedProjectIds)
    {
        var user = await _db.Users
            .Include(u => u.Profile)
            .Include(u => u.Projects)
            .SingleOrDefaultAsync(u => u.UserId == model.UserId);

        if (user == null) return NotFound();

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Address = model.Address;

        if (user.Profile == null)
        {
            user.Profile = new Profile { UserId = user.UserId };
            _db.Profiles.Add(user.Profile);
        }

        user.Profile.Bio = model.Bio;
        user.Profile.Skills = model.Skills;
        user.Profile.Education = model.Education;
        user.Profile.Experience = model.Experience;
        user.Profile.IsPublic = model.IsPublic;

        if (selectedProjectIds != null)
        {
            var chosenProjects = await _db.Projects
                .Where(p => selectedProjectIds.Contains(p.ProjectId))
                .ToListAsync();
            user.Projects = chosenProjects;
        }
        else
        {
            user.Projects.Clear();
        }

        if (profileImage != null && profileImage.Length > 0)
        {
            if (!string.IsNullOrEmpty(user.Profile.ProfilePictureUrl))
            {
                DeletePhysicalFile(user.Profile.ProfilePictureUrl);
            }

            string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
            string filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profileImage.CopyToAsync(stream);
            }
            user.Profile.ProfilePictureUrl = "/images/" + fileName;
        }

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = "Profilen har uppdaterats!";
        return RedirectToAction("Profile");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveProfilePicture()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId)) return RedirectToAction("Login", "Account");

        var user = await _db.Users.Include(u => u.Profile).SingleOrDefaultAsync(u => u.UserId == userId);

        if (user?.Profile != null && !string.IsNullOrEmpty(user.Profile.ProfilePictureUrl))
        {
            DeletePhysicalFile(user.Profile.ProfilePictureUrl);
            user.Profile.ProfilePictureUrl = null;
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Profilbilden har tagits bort.";
        }
        return RedirectToAction("Edit");
    }

    private CVViewModel MapToViewModel(User user)
    {
        return new CVViewModel
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Address = user.Address,
            Bio = user.Profile?.Bio,
            Skills = user.Profile?.Skills,
            Education = user.Profile?.Education,
            Experience = user.Profile?.Experience,
            ViewCount = user.Profile?.ViewCount ?? 0,
            IsPublic = user.Profile?.IsPublic ?? true,
            ProfilePictureUrl = string.IsNullOrEmpty(user.Profile?.ProfilePictureUrl) ? DefaultProfilePic : user.Profile.ProfilePictureUrl,
            Projects = user.Projects?.Select(p => p.Title).ToList() ?? new List<string>()
        };
    }

    private void DeletePhysicalFile(string relativePath)
    {
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath.TrimStart('/'));
        if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
    }

    [AllowAnonymous]
    public async Task<IActionResult> PublicProfile(int id)
    {
        var user = await _db.Users.Include(u => u.Profile).Include(u => u.Projects).SingleOrDefaultAsync(u => u.UserId == id);
        if (user == null || !user.IsActive || (user.Profile != null && !user.Profile.IsPublic)) return NotFound();

        var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserIdStr == null || int.Parse(currentUserIdStr) != user.UserId)
        {
            if (user.Profile != null) { user.Profile.ViewCount++; await _db.SaveChangesAsync(); }
        }
        return View(MapToViewModel(user));
    }
}
