using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FeedbackController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(
            string moduleArea,
            string feedbackType,
            string subject,
            string message,
            string? pageUrl,
            string? deviceInfo,
            int? referenceEntityId)
        {
            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(message))
            {
                return BadRequest(new { success = false, error = "Konu ve açıklama alanları zorunludur." });
            }

            var user = await _userManager.GetUserAsync(User);
            var roles = user != null ? await _userManager.GetRolesAsync(user) : Array.Empty<string>();
            string roleLabel = roles.Any() ? string.Join(", ", roles.Take(3)) : (user?.UserType.ToString() ?? "Kullanıcı");

            var cleanModule = string.IsNullOrWhiteSpace(moduleArea) ? "Genel" : moduleArea.Trim();
            if (cleanModule.Length > 60) cleanModule = cleanModule.Substring(0, 60);

            var cleanType = string.IsNullOrWhiteSpace(feedbackType) ? "Oneri" : feedbackType.Trim();
            if (cleanType.Length > 40) cleanType = cleanType.Substring(0, 40);

            var feedback = new PlatformFeedback
            {
                ModuleArea = cleanModule,
                FeedbackType = cleanType,
                Subject = subject.Trim().Length > 200 ? subject.Trim().Substring(0, 200) : subject.Trim(),
                Message = message.Trim().Length > 4000 ? message.Trim().Substring(0, 4000) : message.Trim(),
                PageUrl = string.IsNullOrWhiteSpace(pageUrl) ? Request.Headers.Referer.ToString() : pageUrl.Trim(),
                DeviceInfo = string.IsNullOrWhiteSpace(deviceInfo) ? Request.Headers.UserAgent.ToString() : deviceInfo.Trim(),
                UserId = user?.Id ?? User.FindFirstValue(ClaimTypes.NameIdentifier),
                UserFullName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : User.Identity?.Name,
                UserEmail = user?.Email ?? User.Identity?.Name,
                UserRoleLabel = roleLabel,
                ReferenceEntityId = referenceEntityId,
                Status = "Yeni",
                CreatedAt = DateTime.UtcNow
            };

            if (feedback.PageUrl != null && feedback.PageUrl.Length > 500)
                feedback.PageUrl = feedback.PageUrl.Substring(0, 500);
            if (feedback.DeviceInfo != null && feedback.DeviceInfo.Length > 300)
                feedback.DeviceInfo = feedback.DeviceInfo.Substring(0, 300);
            if (feedback.UserRoleLabel != null && feedback.UserRoleLabel.Length > 100)
                feedback.UserRoleLabel = feedback.UserRoleLabel.Substring(0, 100);

            _context.PlatformFeedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                id = feedback.Id,
                message = $"Geri bildiriminiz (#{feedback.Id}) Süper Admin ekibine iletildi. Teşekkür ederiz!"
            });
        }
    }
}
