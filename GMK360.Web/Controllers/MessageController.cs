using System.Linq;
using System.Threading.Tasks;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessageController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Inbox()
        {
            var user = await _userManager.GetUserAsync(User);
            var messages = await _context.Messages
                                         .Include(m => m.Sender)
                                         .Where(m => m.ReceiverId == user.Id)
                                         .OrderByDescending(m => m.CreatedAt)
                                         .ToListAsync();
            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> Send(string receiverId, int? propertyId, string content)
        {
            var sender = await _userManager.GetUserAsync(User);
            
            if (string.IsNullOrWhiteSpace(content) || receiverId == null)
            {
                return RedirectToAction(nameof(Inbox));
            }

            var message = new Message
            {
                SenderId = sender.Id,
                ReceiverId = receiverId,
                PropertyId = propertyId,
                Content = content,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Mesajınız gönderildi.";
            return RedirectToAction(nameof(Inbox)); // or to conversation view
        }
    }
}
