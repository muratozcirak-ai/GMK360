using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GMK360.Core.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using GMK360.Core.Entities.Identity;

namespace GMK360.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ModerationController : Controller
    {
        private readonly IPropertyService _propertyService;
        private readonly IEmailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ModerationController(IPropertyService propertyService, IEmailService emailService, UserManager<ApplicationUser> userManager)
        {
            _propertyService = propertyService;
            _emailService = emailService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var pendingProperties = await _propertyService.GetPendingPropertiesAsync();
            return View(pendingProperties);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            await _propertyService.ApprovePropertyAsync(id);
            TempData["SuccessMessage"] = "İlan başarıyla onaylandı ve yayına alındı.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["ErrorMessage"] = "Reddetme sebebi belirtilmelidir.";
                return RedirectToAction(nameof(Index));
            }

            var property = await _propertyService.GetPropertyByIdAsync(id);
            await _propertyService.RejectPropertyAsync(id, reason);
            TempData["SuccessMessage"] = "İlan reddedildi.";
            
            // Red sebebini email ile kullanıcıya bildir
            if (property != null && !string.IsNullOrEmpty(property.UserId))
            {
                var user = await _userManager.FindByIdAsync(property.UserId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    string subject = "İlanınız Onaylanmadı";
                    string body = $"<p>Merhaba {user.FirstName},</p><p><strong>{property.Title}</strong> başlıklı ilanınız aşağıdaki sebepten dolayı onaylanmamıştır:</p><p><em>{reason}</em></p><p>Lütfen ilanınızı düzenleyip tekrar onaya gönderin.</p>";
                    await _emailService.SendEmailAsync(user.Email, subject, body);
                }
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}
