using grupp6WebApp.Data;
using grupp6WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace grupp6WebApp.Controllers
{
    // Controller som hanterar meddelanden
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Konstruktor
        public MessageController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Visar inkorgen för inloggad användare
        [Authorize]
        public async Task<IActionResult> Inbox()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var messages = await _context.Messages
                .Where(m => m.ReceiverUserId == userId)
                .OrderByDescending(m => m.SentDate)
                .ToListAsync();

            return View(messages);
        }

        // Visar ett meddelande + markerar som läst
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.MessageId == id);

            if (message == null || message.ReceiverUserId != userId)
            {
                return NotFound();
            }

            if (!message.IsRead)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return View(message);
        }

        // Visar formulär för nytt meddelande / svar
        [Authorize]
        public async Task<IActionResult> Create(int? receiverUserId)
        {
            ViewBag.Users = await _context.Users.ToListAsync();
            ViewBag.ReceiverUserId = receiverUserId;
            return View();
        }

        // Sparar nytt meddelande
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int receiverUserId,
            string subject,
            string content,
            string? senderName)
        {
            // Inloggad användare → ta namn automatiskt
            senderName = User.Identity!.Name;

            if (string.IsNullOrWhiteSpace(subject))
            {
                ModelState.AddModelError("Subject", "Ämne måste anges");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError("Content", "Meddelande måste anges");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Users = await _context.Users.ToListAsync();
                ViewBag.ReceiverUserId = receiverUserId;
                return View();
            }

            var message = new Message
            {
                ReceiverUserId = receiverUserId,
                SenderName = senderName!,
                Subject = subject,
                Content = content,
                IsRead = false,
                SentDate = DateTime.Now
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Inbox));
        }

        // Bekräftelsesida för borttagning
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.MessageId == id);

            if (message == null || message.ReceiverUserId != userId)
            {
                return NotFound();
            }

            return View(message);
        }

        // Tar bort meddelande
        [Authorize]
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.MessageId == id);

            if (message == null || message.ReceiverUserId != userId)
            {
                return NotFound();
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Inbox));
        }
    }
}
