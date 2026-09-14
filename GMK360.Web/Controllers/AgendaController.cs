using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities.Construction;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class AgendaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public AgendaController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        private int GetCurrentAgencyId()
        {
            var agencyIdClaim = User.FindFirst("AgencyId")?.Value;
            if (int.TryParse(agencyIdClaim, out int agencyId))
            {
                return agencyId;
            }
            return 1; // Fallback
        }

        public async Task<IActionResult> Index(DateTime? selectedDate)
        {
            var agencyId = GetCurrentAgencyId();
            
            // Get all dates that have an event
            var eventDates = await _context.AgendaRecords
                .Where(a => a.AgencyId == agencyId && !a.IsDeleted)
                .Select(a => a.EventDate.Date)
                .Distinct()
                .ToListAsync();

            ViewBag.EventDates = eventDates;

            // Load records for the selected date, or upcoming/all if none selected
            var query = _context.AgendaRecords
                .Include(a => a.Project)
                .Include(a => a.Phonebook)
                .Where(a => a.AgencyId == agencyId && !a.IsDeleted);

            if (selectedDate.HasValue)
            {
                query = query.Where(a => a.EventDate.Date == selectedDate.Value.Date);
                ViewBag.SelectedDate = selectedDate.Value;
            }
            else
            {
                query = query.OrderBy(a => a.EventDate).Take(50); // Show recent/upcoming
            }

            var records = await query.ToListAsync();
            return View(records);
        }

        [HttpGet]
        public async Task<IActionResult> GetDetails(int id)
        {
            var agencyId = GetCurrentAgencyId();
            var record = await _context.AgendaRecords.Include(r => r.Items)
                .Include(a => a.Project)
                .Include(a => a.Phonebook)
                .FirstOrDefaultAsync(a => a.Id == id && a.AgencyId == agencyId);

            if (record == null) return NotFound();
            
            return PartialView("_AgendaDetails", record);
        }

        
        public IActionResult Create()
        {
            var agencyId = GetCurrentAgencyId();
            ViewBag.ProjectId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.ConstructionProjects.Where(p => p.AgencyId == agencyId), "Id", "Name");
            ViewBag.PhonebookId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.AgencyPhonebooks.Where(p => p.AgencyId == agencyId), "Id", "Name");
            // Also multiselect for participants
            ViewBag.ParticipantsList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.AgencyPhonebooks.Where(p => p.AgencyId == agencyId), "Id", "Name");
            return View(new AgendaRecord { EventDate = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AgendaRecord record, IFormFile imageFile, List<string> itemTopicTitle, List<string> itemPresentationText, List<int> selectedParticipants)
        {
            var agencyId = GetCurrentAgencyId();
            record.AgencyId = agencyId;
            record.CreatedAt = DateTime.Now;
            record.IsDeleted = false;
            
            // Ana toplantı resmi (Opsiyonel)
            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "agenda");
                Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }
                record.ImageUrl = "/uploads/agenda/" + uniqueFileName;
            }

            // Gündem Maddeleri (Slaytlar)
            record.Items = new List<AgendaItem>();
            if (itemTopicTitle != null && itemTopicTitle.Count > 0)
            {
                for (int i = 0; i < itemTopicTitle.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(itemTopicTitle[i])) continue;

                    var newItem = new AgendaItem
                    {
                        OrderNo = i + 1,
                        TopicTitle = itemTopicTitle[i],
                        PresentationText = itemPresentationText != null && itemPresentationText.Count > i ? itemPresentationText[i] : "",
                        LiveMeetingNotes = ""
                    };

                    // Her gündem maddesinin kendi resmi
                    var slideImage = Request.Form.Files["itemImage_" + i];
                    if (slideImage != null && slideImage.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "agenda");
                        Directory.CreateDirectory(uploadsFolder);
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + slideImage.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await slideImage.CopyToAsync(fileStream);
                        }
                        newItem.ImageUrl = "/uploads/agenda/" + uniqueFileName;
                    }

                    record.Items.Add(newItem);
                }
            }

            
            // Katılımcılar
            record.Participants = new System.Collections.Generic.List<AgendaParticipant>();
            if (selectedParticipants != null && selectedParticipants.Count > 0)
            {
                foreach(var pId in selectedParticipants)
                {
                    var phonebook = _context.AgencyPhonebooks.FirstOrDefault(p => p.Id == pId);
                    if (phonebook != null)
                    {
                        var participant = new AgendaParticipant { PhonebookId = pId };
                        if (!string.IsNullOrEmpty(phonebook.LinkedUserId))
                        {
                            participant.UserId = phonebook.LinkedUserId;
                        }
                        else
                        {
                            participant.AccessToken = Guid.NewGuid().ToString("N");
                        }
                        record.Participants.Add(participant);
                    }
                }
            }
            
            _context.AgendaRecords.Add(record);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { selectedDate = record.EventDate.ToString("yyyy-MM-dd") });
        }

        [HttpPost]
        public async Task<IActionResult> SaveLiveNotes(int itemId, string notes)
        {
            var item = await _context.AgendaItems.FindAsync(itemId);
            if (item == null) return NotFound();
            
            item.LiveMeetingNotes = notes;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Guest(string token)
        {
            if (string.IsNullOrEmpty(token)) return NotFound();

            var participant = await _context.AgendaParticipants
                .Include(p => p.AgendaRecord)
                .ThenInclude(r => r.Items)
                .FirstOrDefaultAsync(p => p.AccessToken == token);

            if (participant == null) return NotFound();
            
            // Mark as viewed
            if (!participant.IsViewed)
            {
                participant.IsViewed = true;
                await _context.SaveChangesAsync();
            }

            ViewBag.ParticipantId = participant.Id;
            ViewBag.ParticipantNotes = participant.ParticipantNotes;
            return View("GuestView", participant.AgendaRecord);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SaveParticipantNotes(int participantId, string notes)
        {
            var p = await _context.AgendaParticipants.FindAsync(participantId);
            if (p != null)
            {
                p.ParticipantNotes = notes;
                await _context.SaveChangesAsync();
            }
            return Ok();
        }


        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var participant = await _context.AgendaParticipants
                .Include(p => p.AgendaRecord)
                .ThenInclude(r => r.Items)
                .FirstOrDefaultAsync(p => p.AgendaRecordId == id && p.UserId == userId);

            if (participant == null) return NotFound("Bu toplantıya katılma yetkiniz yok veya böyle bir toplantı bulunamadı.");

            if (!participant.IsViewed)
            {
                participant.IsViewed = true;
                await _context.SaveChangesAsync();
            }

            ViewBag.ParticipantId = participant.Id;
            ViewBag.ParticipantNotes = participant.ParticipantNotes;
            return View("GuestView", participant.AgendaRecord);
        }

    }
}

