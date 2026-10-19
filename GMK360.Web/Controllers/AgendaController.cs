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
    public class AgendaStaffOption
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string RoleTitle { get; set; } = string.Empty;
    }

    [Authorize]
    public class AgendaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly GMK360.Web.Services.Sms.ISmsService _smsService;

        public AgendaController(
            ApplicationDbContext context, 
            IWebHostEnvironment hostEnvironment,
            GMK360.Web.Services.Sms.ISmsService smsService)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _smsService = smsService;
        }

        private async Task<int> GetCurrentAgencyIdAsync()
        {
            var agencyIdClaim = User.FindFirst("AgencyId")?.Value;
            if (int.TryParse(agencyIdClaim, out int agencyId))
            {
                return agencyId;
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var consultant = await _context.AgencyConsultants.FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive);
                if (consultant != null)
                {
                    return consultant.AgencyId;
                }
            }

            var lastProject = await _context.ConstructionProjects.OrderByDescending(p => p.Id).FirstOrDefaultAsync();
            if (lastProject != null)
            {
                return lastProject.AgencyId;
            }

            var firstAgency = await _context.Agencies.FirstOrDefaultAsync();
            return firstAgency?.Id ?? 1;
        }

        public async Task<IActionResult> Index(DateTime? selectedDate, int? projectId, string? viewMode)
        {
            var agencyId = await GetCurrentAgencyIdAsync();
            
            // Tüm aktif kayıtları çek
            var allQuery = _context.AgendaRecords
                .Include(a => a.Project)
                .Include(a => a.Phonebook)
                .Where(a => a.AgencyId == agencyId && !a.IsDeleted);

            if (projectId.HasValue)
            {
                allQuery = allQuery.Where(a => a.ProjectId == projectId.Value);
                ViewBag.ActiveProject = await _context.ConstructionProjects.FindAsync(projectId.Value);
            }

            var allRecords = await allQuery.OrderByDescending(a => a.EventDate).ToListAsync();

            // Etkinlik olan tarihler (Takvim işaretleri için)
            var eventDates = allRecords
                .Select(a => a.EventDate.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            ViewBag.EventDates = eventDates;
            ViewBag.AllRecords = allRecords;

            // Şantiye / Projeleri çek (Yapı filtresi için)
            ViewBag.Projects = await _context.ConstructionProjects
                .Where(p => p.AgencyId == agencyId && !p.IsDeleted)
                .OrderBy(p => p.Name)
                .ToListAsync();

            // Aktif Personel & Danışman Listesi (Görev ataması için)
            ViewBag.StaffList = await _context.AgencyConsultants
                .Include(c => c.User)
                .Where(c => c.AgencyId == agencyId && c.IsActive)
                .Select(c => new AgendaStaffOption {
                    UserId = c.UserId,
                    FullName = (c.User.FirstName != null && c.User.LastName != null) 
                        ? (c.User.FirstName + " " + c.User.LastName) 
                        : (c.User.DisplayName ?? c.User.UserName),
                    PhoneNumber = c.User.PhoneNumber ?? "",
                    Role = c.Role.ToString(),
                    RoleTitle = c.Role == GMK360.Core.Entities.AgencyRole.SiteManager ? "Şantiye Şefi" :
                                c.Role == GMK360.Core.Entities.AgencyRole.Engineer ? "Saha Mühendisi" :
                                c.Role == GMK360.Core.Entities.AgencyRole.Architect ? "Mimar" :
                                c.Role == GMK360.Core.Entities.AgencyRole.Owner ? "Firma Yöneticisi" :
                                c.Role == GMK360.Core.Entities.AgencyRole.Purchasing ? "Satın Alma Sorumlusu" : "Personel"
                })
                .ToListAsync();

            // Akıllı Tarih Belirleme:
            // Eğer selectedDate verilmediyse:
            // 1. Önce bugün etkinlik var mı bak,
            // 2. Bugün yoksa en yakın randevu gününü (örn: 22 Ekim) seç ki sayfa dolu ve net açılsın!
            DateTime targetDate;
            if (selectedDate.HasValue)
            {
                targetDate = selectedDate.Value.Date;
            }
            else
            {
                var today = DateTime.Today;
                var closestUpcoming = eventDates.FirstOrDefault(d => d >= today);
                if (closestUpcoming != default)
                {
                    targetDate = closestUpcoming;
                }
                else if (eventDates.Any())
                {
                    targetDate = eventDates.Last();
                }
                else
                {
                    targetDate = today;
                }
            }

            ViewBag.SelectedDate = targetDate;
            ViewBag.PrevDate = targetDate.AddDays(-1);
            ViewBag.NextDate = targetDate.AddDays(1);
            ViewBag.ProjectId = projectId;
            ViewBag.ViewMode = viewMode ?? "timeline"; // timeline (saatli tablo) varsayılan

            // Yaklaşan randevular (Hatırlatmalar paneli için)
            ViewBag.UpcomingRecords = allRecords
                .Where(a => a.EventDate >= DateTime.Today.AddDays(-1) && a.StatusId != GMK360.Core.Entities.Construction.AgendaConstants.StatusTamamlandi)
                .OrderBy(a => a.EventDate)
                .Take(6)
                .ToList();

            // Seçilen güne ait kayıtlar
            var dayRecords = allRecords
                .Where(a => a.EventDate.Date == targetDate)
                .OrderBy(a => a.EventDate)
                .ToList();

            return View(dayRecords);
        }

        [HttpGet]
        public async Task<IActionResult> GetDetails(int id)
        {
            var agencyId = await GetCurrentAgencyIdAsync();
            var record = await _context.AgendaRecords.Include(r => r.Items)
                .Include(a => a.Project)
                .Include(a => a.Phonebook)
                .FirstOrDefaultAsync(a => a.Id == id && a.AgencyId == agencyId);

            if (record == null) return NotFound();
            
            return PartialView("_AgendaDetails", record);
        }

        public async Task<IActionResult> Create()
        {
            var agencyId = await GetCurrentAgencyIdAsync();
            ViewBag.ProjectId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.ConstructionProjects.Where(p => p.AgencyId == agencyId), "Id", "Name");
            
            var phonebooks = _context.AgencyPhonebooks
                .Where(p => p.AgencyId == agencyId)
                .Select(p => new {
                    Id = p.Id,
                    // Eğer Tags alanı doluysa, parantez içinde sonuna ekliyoruz ki Select2 aramasında (örn: Betoncu, Kalıpçı) çıksın.
                    Name = string.IsNullOrEmpty(p.Tags) ? p.Name : p.Name + " [" + p.Tags + "]",
                    GroupName = p.ContactType == 1 ? "Ustalar / Ekipler" :
                                p.ContactType == 2 ? "Taşeron Firmalar" :
                                p.ContactType == 3 ? "Tedarikçiler" : "Müşavir / Ofis / Diğer"
                })
                .OrderBy(p => p.GroupName)
                .ThenBy(p => p.Name)
                .ToList();

            ViewBag.PhonebookId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(phonebooks, "Id", "Name", null, "GroupName");
            ViewBag.ParticipantsList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(phonebooks, "Id", "Name", null, "GroupName");
            
            return View(new AgendaRecord { EventDate = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AgendaRecord record, IFormFile imageFile, List<string> itemTopicTitle, List<string> itemPresentationText, List<int> selectedParticipants)
        {
            var agencyId = await GetCurrentAgencyIdAsync();
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
            record.ImageUrl ??= string.Empty;

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

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, byte statusId)
        {
            var agencyId = await GetCurrentAgencyIdAsync();
            var record = await _context.AgendaRecords.FirstOrDefaultAsync(a => a.Id == id && a.AgencyId == agencyId);
            if (record == null) return NotFound();

            record.StatusId = statusId;
            await _context.SaveChangesAsync();
            return Json(new { success = true, statusId = record.StatusId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTask(
            int recordId, 
            string? assignedUserId, 
            string staffName, 
            string? staffPhone,
            string? taskNote,
            bool sendSms = false)
        {
            var agencyId = await GetCurrentAgencyIdAsync();
            var record = await _context.AgendaRecords
                .Include(r => r.Project)
                .Include(r => r.Phonebook)
                .FirstOrDefaultAsync(r => r.Id == recordId && r.AgencyId == agencyId);

            if (record == null) return NotFound();

            var cleanStaff = string.IsNullOrWhiteSpace(staffName) ? "Şantiye Sorumlusu" : staffName.Trim();
            var cleanPhone = (staffPhone ?? "").Trim();
            var cleanNote = string.IsNullOrWhiteSpace(taskNote) ? "Keşif / teslimat karşılama görevi" : taskNote.Trim();

            // Notes alanındaki görevlendirme bilgisini güncelle veya ekle
            var assignStamp = $"[GÖREVLENDİRME: {cleanStaff} | Tel: {cleanPhone} | Talimat: {cleanNote} | Atanma: {DateTime.Now:dd.MM.yyyy HH:mm}]";
            if (!string.IsNullOrEmpty(record.Notes) && record.Notes.Contains("[GÖREVLENDİRME:"))
            {
                var regex = new System.Text.RegularExpressions.Regex(@"\[GÖREVLENDİRME:.*?\]");
                record.Notes = regex.Replace(record.Notes, assignStamp);
            }
            else
            {
                record.Notes = (record.Notes ?? "") + "\n" + assignStamp;
            }

            // Eğer proje varsa, şantiyenin resmi ConstructionTasks listesine de iş emri aç
            if (record.ProjectId.HasValue && !string.IsNullOrEmpty(assignedUserId))
            {
                var searchKey = record.Title.Length > 25 ? record.Title.Substring(0, 25) : record.Title;
                var existingTask = await _context.ConstructionTasks.FirstOrDefaultAsync(t => 
                    t.ConstructionProjectId == record.ProjectId.Value && 
                    t.Title.Contains(searchKey));

                if (existingTask == null)
                {
                    var task = new ConstructionTask
                    {
                        ConstructionProjectId = record.ProjectId.Value,
                        Title = $"[Saha Karşılama] {record.Title}",
                        Description = $"Saat {record.EventDate:HH:mm} randevusunda tedarikçi/usta sahada karşılanacak. Görevli: {cleanStaff}. Talimat: {cleanNote}",
                        AssignedToUserId = assignedUserId,
                        DueDate = record.EventDate,
                        Status = 0, // Bekliyor
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.ConstructionTasks.Add(task);
                }
                else
                {
                    existingTask.AssignedToUserId = assignedUserId;
                    existingTask.Description = $"Saat {record.EventDate:HH:mm} randevusunda tedarikçi/usta sahada karşılanacak. Görevli: {cleanStaff}. Talimat: {cleanNote}";
                }
            }

            // Eğer SMS gönderimi istenmişse
            if (sendSms && !string.IsNullOrEmpty(cleanPhone))
            {
                try
                {
                    var smsText = $"Sayın {cleanStaff}, {record.EventDate:dd.MM.yyyy HH:mm} şantiye randevusunda {record.Phonebook?.Name ?? "Tedarikçi"} karşılama göreviniz bulunmaktadır. Talimat: {cleanNote}";
                    await _smsService.SmsGonderAsync(cleanPhone, smsText);
                }
                catch { }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Şantiye karşılama görevi başarıyla {cleanStaff} personeline atandı!";
            return RedirectToAction(nameof(Index), new { selectedDate = record.EventDate.ToString("yyyy-MM-dd"), projectId = record.ProjectId });
        }

        [HttpPost]
        public async Task<IActionResult> SendStaffSms(int recordId, string phone, string message)
        {
            if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(message))
            {
                return Json(new { success = false, message = "Telefon numarası ve mesaj metni gereklidir." });
            }

            try
            {
                var result = await _smsService.SmsGonderAsync(phone, message);
                return Json(new { success = true, result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteInspection(
            int recordId, 
            string? resultDecision, 
            string? siteNotes, 
            int? rating, 
            List<IFormFile>? mediaFiles)
        {
            var agencyId = await GetCurrentAgencyIdAsync();
            var record = await _context.AgendaRecords
                .Include(r => r.Project)
                .FirstOrDefaultAsync(r => r.Id == recordId && r.AgencyId == agencyId);

            if (record == null) return NotFound();

            record.StatusId = AgendaConstants.StatusTamamlandi;

            // Sahadan yüklenen fotoğraflar / videolar
            var uploadedPaths = new List<string>();
            if (!string.IsNullOrEmpty(record.ImageUrl))
            {
                uploadedPaths.AddRange(record.ImageUrl.Split(';', StringSplitOptions.RemoveEmptyEntries));
            }

            if (mediaFiles != null && mediaFiles.Count > 0)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "agenda");
                Directory.CreateDirectory(uploadsFolder);
                foreach (var file in mediaFiles)
                {
                    if (file.Length > 0)
                    {
                        string safeName = Path.GetFileNameWithoutExtension(file.FileName).Replace(" ", "_");
                        string ext = Path.GetExtension(file.FileName).ToLower();
                        string uniqueFileName = $"{Guid.NewGuid():N}_{safeName}{ext}";
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        uploadedPaths.Add($"/uploads/agenda/{uniqueFileName}");
                    }
                }
            }
            record.ImageUrl = string.Join(";", uploadedPaths);

            // Tutanak ve Taşeron Puanı
            var starRating = rating.HasValue && rating.Value >= 1 && rating.Value <= 5 ? rating.Value : 5;
            var decision = string.IsNullOrWhiteSpace(resultDecision) ? "Olumlu / Teklif Aşamasına Geçildi" : resultDecision.Trim();
            var stars = new string('★', starRating) + new string('☆', 5 - starRating);
            var tutanakText = $"\n\n📋 [KEŞİF TUTANAĞI - {DateTime.Now:dd.MM.yyyy HH:mm}]\n" +
                              $"Sonuç: {decision}\n" +
                              $"Taşeron Puanı: {stars} ({starRating}/5)\n" +
                              $"Saha Notu: {(string.IsNullOrWhiteSpace(siteNotes) ? "Ek not girilmedi." : siteNotes.Trim())}";

            record.Notes = (record.Notes ?? "") + tutanakText;

            // B2BQuoteInvite varsa teklif notuna da yansıt
            var invite = await _context.B2BQuoteInvites.FirstOrDefaultAsync(i => 
                i.OfferNotes != null && i.OfferNotes.Contains(record.EventDate.ToString("dd.MM.yyyy")));
            if (invite != null)
            {
                invite.OfferNotes += $" | [KEŞİF TAMAMLANDI: {decision} - Puan: {starRating}/5]";
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Saha keşif tutanağı, fotoğraflar ve taşeron puanı başarıyla kaydedildi!";
            return RedirectToAction(nameof(Index), new { selectedDate = record.EventDate.ToString("yyyy-MM-dd"), projectId = record.ProjectId });
        }
    }
}

