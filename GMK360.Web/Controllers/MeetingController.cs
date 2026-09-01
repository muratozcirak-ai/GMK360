using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using System.Security.Claims;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class MeetingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MeetingController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // 1. Yönetici için Toplantı ve Karar Defteri Listesi
        public async Task<IActionResult> Index(int buildingId)
        {
            var userId = GetUserId();
            var building = await _context.Buildings
                .FirstOrDefaultAsync(b => b.Id == buildingId && b.ManagerUserId == userId);

            if (building == null) return NotFound();

            var meetings = await _context.SystemMeetings
                .Include(m => m.Decisions)
                .Where(m => m.ContextType == "Building" && m.ContextId == buildingId && !m.IsDeleted)
                .OrderByDescending(m => m.MeetingDate)
                .ToListAsync();

            ViewBag.Building = building;
            return View(meetings);
        }

        // 2. Yeni Toplantı Oluştur (Ajanda)
        [HttpPost]
        public async Task<IActionResult> CreateMeeting(int buildingId, DateTime meetingDate, string agendaTitle)
        {
            var userId = GetUserId();
            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.Id == buildingId && b.ManagerUserId == userId);
            
            if (building == null) return Unauthorized();

            var meeting = new SystemMeeting
            {
                ContextType = "Building",
                ContextId = buildingId,
                MeetingDate = meetingDate,
                AgendaTitle = agendaTitle,
                IsConcluded = false,
                CreatedAt = DateTime.Now
            };

            _context.SystemMeetings.Add(meeting);
            await _context.SaveChangesAsync();

            // TODO: SMS/Email bildirim motorunu tetikle ve sakinleri haberdar et

            TempData["SuccessMessage"] = "Toplantı başarıyla planlandı ve sakinlere duyuruldu.";
            return RedirectToAction(nameof(Index), new { buildingId = buildingId });
        }

        // 3. Karar Ekle (Toplantı Sonrası Karar Defteri)
        [HttpPost]
        public async Task<IActionResult> AddDecision(int meetingId, string decisionText)
        {
            var userId = GetUserId();
            var meeting = await _context.SystemMeetings
                .FirstOrDefaultAsync(m => m.Id == meetingId);

            if (meeting == null || meeting.ContextType != "Building")
                return Unauthorized();
                
            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.Id == meeting.ContextId && b.ManagerUserId == userId);
            if (building == null)
                return Unauthorized();

            var decision = new SystemMeetingDecision
            {
                SystemMeetingId = meetingId,
                DecisionText = decisionText,
                CreatedAt = DateTime.Now
            };

            _context.SystemMeetingDecisions.Add(decision);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { buildingId = meeting.ContextId });
        }
        
        // 4. Toplantıyı Sonlandır (Kararlar tamamlandı)
        [HttpPost]
        public async Task<IActionResult> ConcludeMeeting(int meetingId)
        {
            var userId = GetUserId();
            var meeting = await _context.SystemMeetings
                .FirstOrDefaultAsync(m => m.Id == meetingId);

            if (meeting == null || meeting.ContextType != "Building")
                return Unauthorized();
                
            var building = await _context.Buildings.FirstOrDefaultAsync(b => b.Id == meeting.ContextId && b.ManagerUserId == userId);
            if (building == null)
                return Unauthorized();

            meeting.IsConcluded = true;
            meeting.UpdatedAt = DateTime.Now;
            
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { buildingId = meeting.ContextId });
        }
    }
}
