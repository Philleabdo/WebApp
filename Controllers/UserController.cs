using grupp6WebApp.Data;
using grupp6WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
// Se till att BCrypt.Net är installerat via NuGet om du får rött under BCrypt nedan

namespace grupp6WebApp.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly ApplicationDbContext _db;

    // Vi har tagit bort IPasswordHasher härifrån
    public UserController(ApplicationDbContext db)
    {
        _db = db;
    }

    // Visar användarens profil
    public async Task<IActionResult> Profile()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _db.Users
            .Include(u => u.Profile)
            .Include(u => u.Projects)
            .SingleOrDefaultAsync(u => u.UserId == userId);

        if (user == null) return NotFound("Användaren hittades inte i databasen.");

        var viewModel = new CVViewModel
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Address = user.Address,
            Phone = user.Phone,
            Bio = user.Profile?.Bio,
            Skills = user.Profile?.Skills,
            Education = user.Profile?.Education,
            Experience = user.Profile?.Experience,
            ViewCount = 0,
            IsPublic = user.Profile?.IsPublic ?? true,
            ProfilePictureUrl = user.Profile?.ProfilePictureUrl,
            Projects = user.Projects.Select(p => p.Title).ToList()
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId)) return RedirectToAction("Login", "Account");

        var user = await _db.Users.Include(u => u.Profile).SingleOrDefaultAsync(u => u.UserId == userId);
        if (user == null) return NotFound();

        ViewBag.AllProjects = await _db.Projects.ToListAsync();

        var viewModel = new CVViewModel
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Address = user.Address,
            Bio = user.Profile?.Bio,
            Skills = user.Profile?.Skills,
            Education = user.Profile?.Education,
            Experience = user.Profile?.Experience,
            IsPublic = user.Profile?.IsPublic ?? true,
            ProfilePictureUrl = user.Profile?.ProfilePictureUrl
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(CVViewModel model, IFormFile? profileImage, int? selectedProjectId)
    {
        var user = await _db.Users
            .Include(u => u.Profile)
            .Include(u => u.Projects)
            .SingleOrDefaultAsync(u => u.UserId == model.UserId);

        if (user == null) return NotFound();

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Address = model.Address;
        user.Phone = model.Phone;

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

        if (selectedProjectId.HasValue && selectedProjectId.Value > 0)
        {
            var project = await _db.Projects.FindAsync(selectedProjectId.Value);
            if (project != null)
            {
                project.UserId = user.UserId;
            }
        }

        if (profileImage != null && profileImage.Length > 0)
        {
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
        TempData["SuccessMessage"] = "Profile updated successfully!";

        return RedirectToAction("Profile");
    }

    public async Task<IActionResult> DownloadXml()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId)) return RedirectToAction("Login", "Account");

        var user = await _db.Users
            .Include(u => u.Profile)
            .Include(u => u.Projects)
            .SingleOrDefaultAsync(u => u.UserId == userId);

        if (user == null) return NotFound();

        var xmlDoc = new System.Xml.XmlDocument();
        var root = xmlDoc.CreateElement("UserProfile");
        xmlDoc.AppendChild(root);

        AddXmlNode(xmlDoc, root, "FullName", $"{user.FirstName} {user.LastName}");
        AddXmlNode(xmlDoc, root, "Email", user.Email);
        AddXmlNode(xmlDoc, root, "Bio", user.Profile?.Bio ?? "");
        AddXmlNode(xmlDoc, root, "Skills", user.Profile?.Skills ?? "");
        AddXmlNode(xmlDoc, root, "Education", user.Profile?.Education ?? "");
        AddXmlNode(xmlDoc, root, "Experience", user.Profile?.Experience ?? "");

        var projectsNode = xmlDoc.CreateElement("Projects");
        root.AppendChild(projectsNode);
        foreach (var p in user.Projects)
        {
            AddXmlNode(xmlDoc, projectsNode, "Project", p.Title);
        }

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(xmlDoc.OuterXml);
        return File(bytes, "application/xml", $"{user.FirstName}_Profile.xml");
    }

    private void AddXmlNode(System.Xml.XmlDocument doc, System.Xml.XmlElement parent, string name, string value)
    {
        var node = doc.CreateElement(name);
        node.InnerText = value;
        parent.AppendChild(node);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateAccount()
    {
        // 1. Hämta den inloggade användarens ID
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId)) return RedirectToAction("Login", "Account");

        // 2. Hämta användaren från databasen
        var user = await _db.Users.FindAsync(userId);

        if (user != null)
        {
            // 3. Sätt IsActive till false istället för att radera
            user.IsActive = false;
            await _db.SaveChangesAsync();

            // 4. Logga ut användaren (eftersom kontot nu är inaktivt)
            // OBS: Dubbelkolla att din Logout-action ligger i Account-controllern
            return RedirectToAction("Logout", "Account");
        }

        return RedirectToAction("Profile");
    }

    [AllowAnonymous] // Gör så att även de som inte är inloggade kan se offentliga profiler
    public async Task<IActionResult> PublicProfile(int id)
    {
        // 1. Hämta användaren och deras profil
        var user = await _db.Users
            .Include(u => u.Profile)
            .Include(u => u.Projects)
            .SingleOrDefaultAsync(u => u.UserId == id);

        // 2. Säkerhetskoll: Om användaren inte finns, är inaktiv eller har privat profil -> Visa ej
        if (user == null || !user.IsActive || (user.Profile != null && !user.Profile.IsPublic))
        {
            return NotFound("Profilen är inte tillgänglig.");
        }

        // 3. Öka ViewCount! 
        // Vi kollar så att det inte är användaren själv som tittar på sin egen profil
        var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (currentUserIdClaim == null || int.Parse(currentUserIdClaim) != user.UserId)
        {
            if (user.Profile != null)
            {
                user.Profile.ViewCount++; // Öka med 1
                await _db.SaveChangesAsync();
            }
        }

        // 4. Mappa till ViewModel (precis som i din vanliga Profile-metod)
        var viewModel = new CVViewModel
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Address = user.Address,
            Phone = user.Phone,
            Bio = user.Profile?.Bio,
            Skills = user.Profile?.Skills,
            Education = user.Profile?.Education,
            Experience = user.Profile?.Experience,
            ViewCount = user.Profile?.ViewCount ?? 0,
            ProfilePictureUrl = user.Profile?.ProfilePictureUrl,
            Projects = user.Projects.Select(p => p.Title).ToList()
        };

        return View(viewModel);
    }

}