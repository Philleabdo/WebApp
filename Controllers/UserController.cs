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

    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId)) return RedirectToAction("Login", "Account");

        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        // --- HÄR ANVÄNDER VI BCRYPT ISTÄLLET FÖR PASSWORDHASHER ---
        bool isValid = BCrypt.Net.BCrypt.Verify(model.OldPassword, user.Password);

        if (!isValid)
        {
            ModelState.AddModelError("OldPassword", "Det gamla lösenordet är felaktigt.");
            return View(model);
        }

        // Hasha det nya lösenordet med BCrypt
        user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        // -----------------------------------------------------------

        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Ditt lösenord har uppdaterats!";
        return RedirectToAction("Profile");
    }
}