using GMK360.Core.Entities;
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

    public partial class AgendaController : Controller

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

            return RedirectToAction(nameof(MeetingRoom), new { id = record.Id });

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

                .Include(p => p.AgendaRecord).ThenInclude(r => r.Project)
                .Include(p => p.AgendaRecord).ThenInclude(r => r.Phonebook)
                .Include(p => p.AgendaRecord).ThenInclude(r => r.Participants)
                .FirstOrDefaultAsync(p => p.AccessToken == token);

            if (participant != null && participant.AgendaRecord != null)
            {
                var gContact = await _context.AgencyPhonebooks.FirstOrDefaultAsync(c => c.Id == participant.PhonebookId);
                var gItemIds = participant.AgendaRecord.Items != null ? participant.AgendaRecord.Items.Select(i => i.Id).ToList() : new System.Collections.Generic.List<int>();
                var gDocs = await _context.DocumentArchives
                    .Where(d => d.SourceModule == "AgendaItem" && d.SourceRecordId.HasValue && gItemIds.Contains(d.SourceRecordId.Value) && !d.IsDeleted)
                    .OrderBy(d => d.Id)
                    .ToListAsync();
                ViewBag.ParticipantToken = participant.AccessToken;
                ViewBag.ParticipantContact = gContact;
                ViewBag.TopicDocuments = gDocs;
            }



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

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuestAddEntry(string token, string? messageText, string? docTitle, IFormFile? mediaFile)
        {
            if (string.IsNullOrWhiteSpace(token)) return NotFound();

            var participant = await _context.AgendaParticipants
                .Include(p => p.AgendaRecord)
                    .ThenInclude(r => r.Items)
                .FirstOrDefaultAsync(p => p.AccessToken == token);

            if (participant == null || participant.AgendaRecord == null) return NotFound();

            var record = participant.AgendaRecord;
            var contact = await _context.AgencyPhonebooks.FirstOrDefaultAsync(c => c.Id == participant.PhonebookId);
            string speakerName = contact != null && !string.IsNullOrWhiteSpace(contact.Name) ? contact.Name : "Katılımcı";

            bool hasText = !string.IsNullOrWhiteSpace(messageText);
            bool hasFile = mediaFile != null && mediaFile.Length > 0;

            if (!hasText && !hasFile)
            {
                TempData["GuestError"] = "Lütfen bir soru/açıklama yazın veya bir fotoğraf/video/evrak seçin.";
                return RedirectToAction(nameof(Guest), new { token = token });
            }

            int nextOrder = (record.Items != null && record.Items.Any() ? record.Items.Max(i => i.OrderNo) : 0) + 1;
            string cleanTitle = !string.IsNullOrWhiteSpace(docTitle)
                ? docTitle.Trim()
                : $"{speakerName} Eki ({DateTime.Now:dd.MM.yyyy HH:mm})";

            string statement = hasText
                ? $"[{speakerName}]: {messageText!.Trim()}"
                : $"[{speakerName} tarafından '{cleanTitle}' paylaşıldı]";

            var newItem = new AgendaItem
            {
                AgendaRecordId = record.Id,
                TopicTitle = "Katılımcı",
                PresentationText = statement,
                LiveMeetingNotes = "",
                ImageUrl = "",
                OrderNo = nextOrder
            };

            _context.AgendaItems.Add(newItem);
            await _context.SaveChangesAsync();

            if (hasFile && mediaFile != null)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "agenda");
                Directory.CreateDirectory(uploadsFolder);
                string safeExt = Path.GetExtension(mediaFile.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(safeExt)) safeExt = ".jpg";
                string uniqueFileName = Guid.NewGuid().ToString("N") + safeExt;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await mediaFile.CopyToAsync(fileStream);
                }

                string docUrl = "/uploads/agenda/" + uniqueFileName;
                var doc = new DocumentArchive
                {
                    AgencyId = record.AgencyId,
                    ProjectId = record.ProjectId,
                    SourceModule = "AgendaItem",
                    SourceRecordId = newItem.Id,
                    Title = cleanTitle,
                    FileName = cleanTitle + safeExt,
                    FileExtension = safeExt,
                    DocumentUrl = docUrl,
                    Category = "Alınan Belge",
                    UploadDate = DateTime.Now,
                    Status = "Tamamlandı",
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };

                _context.DocumentArchives.Add(doc);
                await _context.SaveChangesAsync();
            }

            TempData["GuestSuccess"] = "Mesajınız ve ekleriniz canlı konu masasına başarıyla iletildi.";
            return RedirectToAction(nameof(Guest), new { token = token });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuestRequestNextSession(string token, string requestedDate, string? requestedTime, string? requestNote)
        {
            if (string.IsNullOrWhiteSpace(token)) return NotFound();

            var participant = await _context.AgendaParticipants
                .Include(p => p.AgendaRecord)
                    .ThenInclude(r => r.Items)
                .FirstOrDefaultAsync(p => p.AccessToken == token);

            if (participant == null || participant.AgendaRecord == null) return NotFound();

            var record = participant.AgendaRecord;
            var contact = await _context.AgencyPhonebooks.FirstOrDefaultAsync(c => c.Id == participant.PhonebookId);
            string speakerName = contact != null && !string.IsNullOrWhiteSpace(contact.Name) ? contact.Name : "Katılımcı";

            DateTime targetDate = DateTime.Today.AddDays(2).AddHours(14);
            if (!string.IsNullOrWhiteSpace(requestedDate) && DateTime.TryParse(requestedDate, out var d))
            {
                if (!string.IsNullOrWhiteSpace(requestedTime) && TimeSpan.TryParse(requestedTime, out var t))
                {
                    targetDate = d.Date.Add(t);
                }
                else
                {
                    targetDate = d.Date.AddHours(14);
                }
            }

            int nextOrder = (record.Items != null && record.Items.Any() ? record.Items.Max(i => i.OrderNo) : 0) + 1;
            string notePart = !string.IsNullOrWhiteSpace(requestNote) ? $" — Not: {requestNote.Trim()}" : "";
            string reqText = $"📅 [2. RANDEVU / DEVAM OTURUMU TALEBİ - {speakerName}]: {targetDate:dd.MM.yyyy} Saat {targetDate:HH:mm} için yeni görüşme talep edildi.{notePart}";

            var newItem = new AgendaItem
            {
                AgendaRecordId = record.Id,
                TopicTitle = "Katılımcı",
                PresentationText = reqText,
                LiveMeetingNotes = "",
                ImageUrl = "",
                OrderNo = nextOrder
            };

            _context.AgendaItems.Add(newItem);
            record.Notes = (record.Notes ?? "") + $"\n{reqText}";
            await _context.SaveChangesAsync();

            TempData["GuestSuccess"] = $"{targetDate:dd.MM.yyyy} saat {targetDate:HH:mm} için randevu talebiniz firma yetkilisine iletildi.";
            return RedirectToAction(nameof(Guest), new { token = token });
        }

        private async Task<IActionResult> CleanupUnused(AgendaParticipant p)
        {
            if (false)
            {

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



        // ==========================================

        // TOPLANTI & KARAR MASASI (YENİ ODA VE MÜZAKERE MODU)

        // ==========================================

        [HttpGet]

        public async Task<IActionResult> Meetings(int? projectId, byte? statusId)

        {

            var agencyId = await GetCurrentAgencyIdAsync();

            var query = _context.AgendaRecords

                .Include(a => a.Project)

                .Include(a => a.Phonebook)

                .Include(a => a.Items)

                .Include(a => a.Participants)

                .Where(a => a.AgencyId == agencyId && !a.IsDeleted);



            if (projectId.HasValue)

                query = query.Where(a => a.ProjectId == projectId.Value);



            if (statusId.HasValue)

                query = query.Where(a => a.StatusId == statusId.Value);



            var records = await query.OrderByDescending(a => a.EventDate).ToListAsync();



            ViewBag.Projects = await _context.ConstructionProjects

                .Where(p => p.AgencyId == agencyId && !p.IsDeleted)

                .OrderBy(p => p.Name)

                .ToListAsync();



            ViewBag.SelectedProjectId = projectId;

            ViewBag.SelectedStatusId = statusId;



            return View(records);

        }



        [HttpGet]

        public async Task<IActionResult> MeetingRoom(int id)

        {

            var agencyId = await GetCurrentAgencyIdAsync();

            var record = await _context.AgendaRecords

                .Include(a => a.Project)

                .Include(a => a.Phonebook)

                .Include(a => a.Items)

                .Include(a => a.Participants)

                .FirstOrDefaultAsync(a => a.Id == id && a.AgencyId == agencyId && !a.IsDeleted);



            if (record == null) return NotFound();



            if (record.PhonebookId.HasValue && record.PhonebookId.Value > 0 && !record.Participants.Any(p => p.PhonebookId == record.PhonebookId.Value))
            {
                var mainParticipant = new AgendaParticipant
                {
                    AgendaRecordId = record.Id,
                    PhonebookId = record.PhonebookId.Value,
                    AccessToken = Guid.NewGuid().ToString("N")
                };
                _context.AgendaParticipants.Add(mainParticipant);
                record.Participants.Add(mainParticipant);
                await _context.SaveChangesAsync();
            }
            bool tokenUpdated = false;
            foreach (var part in record.Participants)
            {
                if (string.IsNullOrWhiteSpace(part.AccessToken))
                {
                    part.AccessToken = Guid.NewGuid().ToString("N");
                    tokenUpdated = true;
                }
            }
            if (tokenUpdated)
            {
                await _context.SaveChangesAsync();
            }
            var phonebookIds = record.Participants.Select(p => p.PhonebookId).ToList();
            var participantsPhonebook = await _context.AgencyPhonebooks
                .Where(p => phonebookIds.Contains(p.Id))
                .ToListAsync();
            ViewBag.ParticipantsPhonebook = participantsPhonebook;

            // Rehberde kayıtlı olup toplantıya henüz eklenmemiş kişiler
            ViewBag.AvailablePhonebooks = await _context.AgencyPhonebooks
                .Where(p => p.AgencyId == agencyId && !phonebookIds.Contains(p.Id) && !p.IsDeleted)
                .OrderBy(p => p.Name)
                .ToListAsync();

            // Bu toplantının konularına ait yüklenen belgeler
            var itemIds = record.Items.Select(i => i.Id).ToList();
            ViewBag.TopicDocuments = await _context.DocumentArchives
                .Where(d => d.AgencyId == agencyId && d.SourceModule == "AgendaItem" && d.SourceRecordId.HasValue && itemIds.Contains(d.SourceRecordId.Value) && !d.IsDeleted)
                .ToListAsync();

            return View(record);

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> AddTopic(int recordId, string topicTitle, string? presentationText, IFormFile? topicImage)

        {

            var agencyId = await GetCurrentAgencyIdAsync();

            var record = await _context.AgendaRecords

                .Include(r => r.Items)

                .FirstOrDefaultAsync(r => r.Id == recordId && r.AgencyId == agencyId && !r.IsDeleted);



            if (record == null) return NotFound();



            string? imageUrl = null;

            if (topicImage != null && topicImage.Length > 0)

            {

                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "agenda");

                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString("N") + "_" + Path.GetFileName(topicImage.FileName);

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))

                {

                    await topicImage.CopyToAsync(fileStream);

                }

                imageUrl = "/uploads/agenda/" + uniqueFileName;

            }



            int nextOrder = (record.Items != null && record.Items.Any() ? record.Items.Max(i => i.OrderNo) : 0) + 1;

            var newItem = new AgendaItem

            {

                AgendaRecordId = recordId,

                TopicTitle = topicTitle,

                PresentationText = presentationText ?? "",

                ImageUrl = imageUrl,

                OrderNo = nextOrder,

                LiveMeetingNotes = ""

            };



            _context.AgendaItems.Add(newItem);

            await _context.SaveChangesAsync();



            TempData["SuccessMessage"] = $"'{topicTitle}' gündem maddesi toplantıya eklendi.";

            return RedirectToAction(nameof(MeetingRoom), new { id = recordId });

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> ConcludeMeeting(int recordId, string? finalSummary)

        {

            var agencyId = await GetCurrentAgencyIdAsync();

            var record = await _context.AgendaRecords.FirstOrDefaultAsync(r => r.Id == recordId && r.AgencyId == agencyId && !r.IsDeleted);

            if (record == null) return NotFound();



            record.StatusId = AgendaConstants.StatusTamamlandi;

            if (!string.IsNullOrWhiteSpace(finalSummary))

            {

                record.Notes = (record.Notes ?? "") + "\n\n📋 [TOPLANTI SONUÇ BİLDİRGESİ & ALINAN NİHAİ KARARLAR - " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "]\n" + finalSummary.Trim();

            }

            await _context.SaveChangesAsync();



            TempData["SuccessMessage"] = "Toplantı başarıyla sonuçlandırıldı ve alınan kararlar deftere işlendi.";

            return RedirectToAction(nameof(MeetingRoom), new { id = recordId });

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> ReopenMeeting(int recordId)

        {

            var agencyId = await GetCurrentAgencyIdAsync();

            var record = await _context.AgendaRecords.FirstOrDefaultAsync(r => r.Id == recordId && r.AgencyId == agencyId && !r.IsDeleted);

            if (record == null) return NotFound();



            record.StatusId = AgendaConstants.StatusPlanlandi;

            await _context.SaveChangesAsync();



            TempData["SuccessMessage"] = "Toplantı yeniden canlı müzakere moduna alındı.";

            return RedirectToAction(nameof(MeetingRoom), new { id = recordId });

        }



        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> ScheduleNextSession(int recordId, string nextSessionDate, string? nextSessionTime, string? nextSessionNote, bool createNewSession = false)

        {

            var agencyId = await GetCurrentAgencyIdAsync();

            var record = await _context.AgendaRecords

                .Include(r => r.Participants)

                .FirstOrDefaultAsync(r => r.Id == recordId && r.AgencyId == agencyId && !r.IsDeleted);



            if (record == null) return NotFound();

            DateTime targetSessionDate = DateTime.Now.AddDays(3);
            if (!string.IsNullOrWhiteSpace(nextSessionDate) && DateTime.TryParse(nextSessionDate, out var pDate))
            {
                if (!string.IsNullOrWhiteSpace(nextSessionTime) && TimeSpan.TryParse(nextSessionTime, out var pTime))
                {
                    targetSessionDate = pDate.Date + pTime;
                }
                else
                {
                    targetSessionDate = pDate.Date.AddHours(14);
                }
            }



            if (createNewSession)

            {

                record.StatusId = AgendaConstants.StatusTamamlandi;

                record.Notes = (record.Notes ?? "") + $"\n[1. Oturum Tamamlandı. Devam Oturumu: {targetSessionDate:dd.MM.yyyy HH:mm}]";



                string newTitle = record.Title.Contains("Oturum") 

                    ? record.Title.Replace("1. Oturum", "2. Oturum").Replace("2. Oturum", "3. Oturum")

                    : record.Title + " (2. Devam Oturumu)";



                var newRecord = new AgendaRecord

                {

                    AgencyId = agencyId,

                    Title = newTitle,

                    Notes = $"[ÖNCEKİ OTURUMDAN DEVAM EDEN MÜZAKERE]\n" + (nextSessionNote ?? ""),

                    EventDate = targetSessionDate,

                    RecordTypeId = record.RecordTypeId,

                    StatusId = AgendaConstants.StatusPlanlandi,

                    ProjectId = record.ProjectId,

                    PhonebookId = record.PhonebookId,

                    ImageUrl = record.ImageUrl,

                    CreatedAt = DateTime.Now,

                    IsDeleted = false,

                    Participants = new List<AgendaParticipant>()

                };



                if (record.Participants != null && record.Participants.Any())

                {

                    foreach (var p in record.Participants)

                    {

                        newRecord.Participants.Add(new AgendaParticipant

                        {

                            PhonebookId = p.PhonebookId,

                            UserId = p.UserId,

                            AccessToken = Guid.NewGuid().ToString("N")

                        });

                    }

                }



                _context.AgendaRecords.Add(newRecord);

                await _context.SaveChangesAsync();



                TempData["SuccessMessage"] = $"Devam oturumu başarıyla planlandı: {targetSessionDate:dd.MM.yyyy HH:mm}";

                return RedirectToAction(nameof(MeetingRoom), new { id = newRecord.Id });

            }

            else

            {

                record.EventDate = targetSessionDate;

                record.StatusId = AgendaConstants.StatusPlanlandi;

                if (!string.IsNullOrWhiteSpace(nextSessionNote))

                {

                    record.Notes = (record.Notes ?? "") + $"\n[YENİ DEVAM TARİHİ ATANDI: {targetSessionDate:dd.MM.yyyy HH:mm} - Not: {nextSessionNote.Trim()}]";

                }

                await _context.SaveChangesAsync();



                TempData["SuccessMessage"] = $"Toplantı tarihi {targetSessionDate:dd.MM.yyyy HH:mm} olarak güncellendi.";

                return RedirectToAction(nameof(MeetingRoom), new { id = record.Id });

            }

        }

    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMeetingParticipant(int recordId, int phonebookId)
        {
            var agencyId = await GetCurrentAgencyIdAsync();
            var record = await _context.AgendaRecords
                .Include(r => r.Participants)
                .FirstOrDefaultAsync(r => r.Id == recordId && r.AgencyId == agencyId && !r.IsDeleted);

            if (record == null) return NotFound();

            if (!record.Participants.Any(p => p.PhonebookId == phonebookId))
            {
                var phonebook = await _context.AgencyPhonebooks.FirstOrDefaultAsync(p => p.Id == phonebookId && p.AgencyId == agencyId);
                var participant = new AgendaParticipant
                {
                    AgendaRecordId = recordId,
                    PhonebookId = phonebookId,
                    UserId = phonebook?.LinkedUserId,
                    AccessToken = Guid.NewGuid().ToString("N")
                };
                _context.AgendaParticipants.Add(participant);
                await _context.SaveChangesAsync();
                if (!record.PhonebookId.HasValue || record.PhonebookId.Value <= 0)
                {
                    record.PhonebookId = phonebookId;
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Katılımcı toplantıya başarıyla eklendi ve canlı konu bağlantısı oluşturuldu.";
            }

            return RedirectToAction(nameof(MeetingRoom), new { id = recordId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickCreateParticipant(int recordId, string name, string phoneNumber, string? role)
        {
            var agencyId = await GetCurrentAgencyIdAsync();
            var record = await _context.AgendaRecords
                .Include(r => r.Participants)
                .FirstOrDefaultAsync(r => r.Id == recordId && r.AgencyId == agencyId && !r.IsDeleted);

            if (record == null) return NotFound();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phoneNumber))
            {
                TempData["ErrorMessage"] = "Ad Soyad ve Telefon zorunludur.";
                return RedirectToAction(nameof(MeetingRoom), new { id = recordId });
            }

            var newContact = new AgencyPhonebook
            {
                AgencyId = agencyId,
                Name = name.Trim(),
                PhoneNumber = phoneNumber.Trim(),
                ContactType = 4, // Diğer / Katılımcı
                Tags = !string.IsNullOrWhiteSpace(role) ? role.Trim() : "Katılımcı",
                Notes = $"Müzakere Masasında Hızlı Kaydedildi (#{recordId} nolu toplantı)",
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };
            _context.AgencyPhonebooks.Add(newContact);
            await _context.SaveChangesAsync();

            var participant = new AgendaParticipant
            {
                AgendaRecordId = recordId,
                PhonebookId = newContact.Id,
                AccessToken = Guid.NewGuid().ToString("N")
            };
            _context.AgendaParticipants.Add(participant);
            await _context.SaveChangesAsync();

            if (!record.PhonebookId.HasValue || record.PhonebookId.Value <= 0)
            {
                record.PhonebookId = newContact.Id;
                await _context.SaveChangesAsync();
            }
            TempData["SuccessMessage"] = $"{newContact.Name} rehbere ve toplantıya başarıyla eklendi.";
            return RedirectToAction(nameof(MeetingRoom), new { id = recordId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadTopicDocument(int topicId, int recordId, IFormFile file)
        {
            var agencyId = await GetCurrentAgencyIdAsync();
            var record = await _context.AgendaRecords.FirstOrDefaultAsync(r => r.Id == recordId && r.AgencyId == agencyId && !r.IsDeleted);
            if (record == null) return NotFound();

            if (file != null && file.Length > 0)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "agenda");
                Directory.CreateDirectory(uploadsFolder);
                string safeExt = Path.GetExtension(file.FileName).ToLower();
                string uniqueFileName = Guid.NewGuid().ToString("N") + "_" + Path.GetFileName(file.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var doc = new DocumentArchive
                {
                    AgencyId = agencyId,
                    ProjectId = record.ProjectId,
                    SourceModule = "AgendaItem",
                    SourceRecordId = topicId,
                    Title = Path.GetFileNameWithoutExtension(file.FileName),
                    FileName = file.FileName,
                    FileExtension = safeExt,
                    DocumentUrl = "/uploads/agenda/" + uniqueFileName,
                    Category = "Toplantı Belgesi",
                    UploadDate = DateTime.Now,
                    Status = "Tamamlandı",
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };

                _context.DocumentArchives.Add(doc);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Belge konuya başarıyla eklendi.";
            }

            return RedirectToAction(nameof(MeetingRoom), new { id = recordId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveGridRowAuto(int? itemId, int recordId, string speaker, string note, int? orderNo)
        {
            var agencyId = await GetCurrentAgencyIdAsync();
            var record = await _context.AgendaRecords
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == recordId && r.AgencyId == agencyId && !r.IsDeleted);

            if (record == null) return Json(new { success = false, message = "Kayıt bulunamadı." });

            string speakerVal = string.IsNullOrWhiteSpace(speaker) ? "Katılımcı" : speaker.Trim();
            string noteVal = note ?? "";

            AgendaItem item;
            if (itemId.HasValue && itemId.Value > 0)
            {
                item = await _context.AgendaItems.FirstOrDefaultAsync(i => i.Id == itemId.Value && i.AgendaRecordId == recordId);
                if (item == null) return Json(new { success = false, message = "Satır bulunamadı." });
                item.TopicTitle = speakerVal;
                item.PresentationText = noteVal;
                if (orderNo.HasValue) item.OrderNo = orderNo.Value;
            }
            else
            {
                int nextOrder = orderNo ?? ((record.Items != null && record.Items.Any() ? record.Items.Max(i => i.OrderNo) : 0) + 1);
                item = new AgendaItem
                {
                    AgendaRecordId = recordId,
                    TopicTitle = speakerVal ?? "",
                    PresentationText = noteVal ?? "",
                    LiveMeetingNotes = "",
                    ImageUrl = "",
                    OrderNo = nextOrder
                };
                _context.AgendaItems.Add(item);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, itemId = item.Id });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UploadGridDocumentAuto(int itemId, int recordId, string direction, IFormFile file, string? customTitle = null, string? goldenDocType = null)
        {
            var agencyId = await GetCurrentAgencyIdAsync();
            var record = await _context.AgendaRecords
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == recordId && r.AgencyId == agencyId && !r.IsDeleted);
            if (record == null || file == null || file.Length == 0)
                return Json(new { success = false, message = "Geçersiz istek." });

            if (itemId <= 0)
            {
                int nextOrder = (record.Items != null && record.Items.Any() ? record.Items.Max(i => i.OrderNo) : 0) + 1;
                var newItem = new AgendaItem
                {
                    AgendaRecordId = recordId,
                    TopicTitle = direction?.ToLower() == "gönder" ? "Biz" : "Katılımcı",
                    PresentationText = "",
                    LiveMeetingNotes = "",
                    ImageUrl = "",
                    OrderNo = nextOrder
                };
                _context.AgendaItems.Add(newItem);
                await _context.SaveChangesAsync();
                itemId = newItem.Id;
            }

            string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "agenda");
            Directory.CreateDirectory(uploadsFolder);
            string safeExt = Path.GetExtension(file.FileName).ToLower();
            string uniqueFileName = Guid.NewGuid().ToString("N") + "_" + Path.GetFileName(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            string docUrl = "/uploads/agenda/" + uniqueFileName;
            string category = direction?.ToLower() == "gönder" ? "Gönderilen Belge" : "Alınan Belge";
            string rawBaseName = Path.GetFileNameWithoutExtension(file.FileName);
            if (rawBaseName.StartsWith("kamera_evrak_", StringComparison.OrdinalIgnoreCase))
            {
                rawBaseName = $"Kamera Çekimi ({DateTime.Now:dd.MM.yyyy HH:mm})";
            }
            string cleanTitle = !string.IsNullOrWhiteSpace(customTitle) ? customTitle.Trim() : rawBaseName;
            string cleanFileName = cleanTitle + safeExt;

            var customDoc = new DocumentArchive
            {
                AgencyId = agencyId,
                ProjectId = record.ProjectId,
                SourceModule = "AgendaItem",
                SourceRecordId = itemId,
                Title = cleanTitle,
                FileName = cleanFileName,
                FileExtension = safeExt,
                DocumentUrl = docUrl,
                Category = category,
                UploadDate = DateTime.Now,
                Status = "Tamamlandı",
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };
            _context.DocumentArchives.Add(customDoc);

            if (record.ProjectId.HasValue && record.ProjectId.Value > 0)
            {
                var proj = await _context.ConstructionProjects.FirstOrDefaultAsync(p => p.Id == record.ProjectId.Value && !p.IsDeleted);
                if (proj != null)
                {
                    var gType = (goldenDocType ?? "").Trim().ToLowerInvariant();
                    var titleLower = cleanTitle.ToLowerInvariant();
                    var rentPolicy = proj.RentAndRontgenPolicy;
                    bool policyChanged = false;

                    if (gType == "imar" || titleLower.Contains("imar"))
                    {
                        rentPolicy.ImarCapiDocumentUrl = docUrl;
                        policyChanged = true;
                    }
                    else if (gType == "karar" || titleLower.Contains("karar defteri"))
                    {
                        rentPolicy.KararDefteriDocumentUrl = docUrl;
                        policyChanged = true;
                    }
                    else if (gType == "bina_foto" || titleLower.Contains("bina cephe") || titleLower.Contains("sokak foto"))
                    {
                        if (string.IsNullOrWhiteSpace(proj.CurrentStateImageUrl))
                        {
                            proj.CurrentStateImageUrl = docUrl;
                        }
                    }

                    if (policyChanged)
                    {
                        proj.RentPolicyJson = System.Text.Json.JsonSerializer.Serialize(rentPolicy);
                    }
                }
            }

            await _context.SaveChangesAsync();

            bool isVid = safeExt == ".mp4" || safeExt == ".mov" || safeExt == ".webm" || safeExt == ".avi" || safeExt == ".mkv";
            if (true)
            {
                return Json(new
                {
                    success = true,
                    itemId = itemId,
                    docId = customDoc.Id,
                    title = customDoc.Title,
                    fileName = customDoc.FileName,
                    fileUrl = customDoc.DocumentUrl,
                    category = customDoc.Category,
                    isImage = safeExt == ".jpg" || safeExt == ".jpeg" || safeExt == ".png" || safeExt == ".webp",
                    isVideo = isVid,
                    isPdf = safeExt == ".pdf"
                });
            }

            var doc = new DocumentArchive
            {
                AgencyId = agencyId,
                ProjectId = record.ProjectId,
                SourceModule = "AgendaItem",
                SourceRecordId = itemId,
                Title = Path.GetFileNameWithoutExtension(file.FileName),
                FileName = file.FileName,
                FileExtension = safeExt,
                DocumentUrl = docUrl,
                Category = category,
                UploadDate = DateTime.Now,
                Status = "Tamamlandı",
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            _context.DocumentArchives.Add(doc);
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                docId = doc.Id, 
                fileName = doc.FileName, 
                fileUrl = doc.DocumentUrl,
                category = doc.Category,
                isImage = safeExt == ".jpg" || safeExt == ".jpeg" || safeExt == ".png" || safeExt == ".webp",
                isPdf = safeExt == ".pdf"
            });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        [HttpGet]
        public async Task<IActionResult> GetArchiveDocuments(int? projectId)
        {
            var agencyId = await GetCurrentAgencyIdAsync();
            var query = _context.DocumentArchives
                .Include(d => d.Project)
                .Where(d => d.AgencyId == agencyId && !d.IsDeleted && (d.SourceModule == null || d.SourceModule != "AgendaItem"));

            var list = await query
                .OrderByDescending(d => d.Id)
                .Select(d => new {
                    id = d.Id,
                    title = !string.IsNullOrWhiteSpace(d.Title) ? d.Title : d.FileName,
                    fileName = d.FileName,
                    category = !string.IsNullOrWhiteSpace(d.Category) ? d.Category : "Genel Arşiv",
                    documentUrl = d.DocumentUrl,
                    fileExtension = d.FileExtension,
                    projectId = d.ProjectId,
                    projectName = d.Project != null ? d.Project.Name : "Kurumsal Arşiv",
                    uploadDate = d.UploadDate.ToString("dd.MM.yyyy")
                })
                .Take(100)
                .ToListAsync();

            return Json(list);
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> BindArchiveDocumentToRow(int itemId, int recordId, int archiveDocId)
        {
            var agencyId = await GetCurrentAgencyIdAsync();
            var record = await _context.AgendaRecords
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == recordId && r.AgencyId == agencyId && !r.IsDeleted);

            if (record == null) return Json(new { success = false, message = "Toplantı bulunamadı." });

            var masterDoc = await _context.DocumentArchives.FirstOrDefaultAsync(d => d.Id == archiveDocId && d.AgencyId == agencyId && !d.IsDeleted);
            if (masterDoc == null) return Json(new { success = false, message = "Arşiv belgesi bulunamadı." });

            if (itemId <= 0)
            {
                int nextOrder = (record.Items != null && record.Items.Any() ? record.Items.Max(i => i.OrderNo) : 0) + 1;
                var newItem = new AgendaItem
                {
                    AgendaRecordId = recordId,
                    TopicTitle = "Biz",
                    PresentationText = "",
                    LiveMeetingNotes = "",
                    ImageUrl = "",
                    OrderNo = nextOrder
                };
                _context.AgendaItems.Add(newItem);
                await _context.SaveChangesAsync();
                itemId = newItem.Id;
            }

            var linkedDoc = new DocumentArchive
            {
                AgencyId = agencyId,
                ProjectId = record.ProjectId,
                SourceModule = "AgendaItem",
                SourceRecordId = itemId,
                Title = masterDoc.Title,
                FileName = masterDoc.FileName,
                FileExtension = masterDoc.FileExtension,
                DocumentUrl = masterDoc.DocumentUrl,
                Category = "Gönderilen Belge",
                UploadDate = DateTime.Now,
                Status = "Tamamlandı",
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            _context.DocumentArchives.Add(linkedDoc);
            await _context.SaveChangesAsync();

            bool isPdf = masterDoc.FileExtension != null && masterDoc.FileExtension.ToLower().Contains("pdf");
            bool isImg = masterDoc.FileExtension != null && (masterDoc.FileExtension.ToLower().Contains("jpg") || masterDoc.FileExtension.ToLower().Contains("png") || masterDoc.FileExtension.ToLower().Contains("jpeg") || masterDoc.FileExtension.ToLower().Contains("webp"));

            return Json(new {
                success = true,
                itemId = itemId,
                docId = linkedDoc.Id,
                fileName = linkedDoc.FileName,
                fileUrl = linkedDoc.DocumentUrl,
                isPdf = isPdf,
                isImage = isImg
            });
        }

        public async Task<IActionResult> DeleteGridDocumentAuto(int documentId)
        {
            var agencyId = await GetCurrentAgencyIdAsync();
            var doc = await _context.DocumentArchives.FirstOrDefaultAsync(d => d.Id == documentId && d.AgencyId == agencyId && !d.IsDeleted);
            if (doc != null)
            {
                doc.IsDeleted = true;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Belge bulunamadı." });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DeleteGridRowAuto(int itemId, int recordId)
        {
            var agencyId = await GetCurrentAgencyIdAsync();
            var item = await _context.AgendaItems.FirstOrDefaultAsync(i => i.Id == itemId && i.AgendaRecordId == recordId);
            if (item != null)
            {
                _context.AgendaItems.Remove(item);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Satır bulunamadı." });
        }

        public async Task<IActionResult> AddGridRow(int recordId, string speaker, string statement, string? technicalNote, IFormFile? documentFile)
        {
            var agencyId = await GetCurrentAgencyIdAsync();
            var record = await _context.AgendaRecords
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == recordId && r.AgencyId == agencyId && !r.IsDeleted);

            if (record == null) return NotFound();

            int nextOrder = (record.Items != null && record.Items.Any() ? record.Items.Max(i => i.OrderNo) : 0) + 1;

            string speakerVal = string.IsNullOrWhiteSpace(speaker) ? "Katılımcı" : speaker.Trim();
            string statementVal = string.IsNullOrWhiteSpace(statement) ? "-" : statement.Trim();

            var newItem = new AgendaItem
            {
                AgendaRecordId = recordId,
                TopicTitle = speakerVal,
                PresentationText = statementVal,
                LiveMeetingNotes = technicalNote?.Trim() ?? "",
                OrderNo = nextOrder,
                ImageUrl = null
            };

            _context.AgendaItems.Add(newItem);
            await _context.SaveChangesAsync();

            if (documentFile != null && documentFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "agenda");
                Directory.CreateDirectory(uploadsFolder);
                string safeExt = Path.GetExtension(documentFile.FileName).ToLower();
                string uniqueFileName = Guid.NewGuid().ToString("N") + "_" + Path.GetFileName(documentFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await documentFile.CopyToAsync(fileStream);
                }

                string docUrl = "/uploads/agenda/" + uniqueFileName;
                if (safeExt == ".jpg" || safeExt == ".jpeg" || safeExt == ".png" || safeExt == ".webp")
                {
                    newItem.ImageUrl = docUrl;
                }

                var doc = new DocumentArchive
                {
                    AgencyId = agencyId,
                    ProjectId = record.ProjectId,
                    SourceModule = "AgendaItem",
                    SourceRecordId = newItem.Id,
                    Title = Path.GetFileNameWithoutExtension(documentFile.FileName),
                    FileName = documentFile.FileName,
                    FileExtension = safeExt,
                    DocumentUrl = docUrl,
                    Category = "Toplantı Belgesi",
                    UploadDate = DateTime.Now,
                    Status = "Tamamlandı",
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };

                _context.DocumentArchives.Add(doc);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Not müzakere matrisine eklendi.";
            return RedirectToAction(nameof(MeetingRoom), new { id = recordId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGridRow(int itemId, int recordId)
        {
            var agencyId = await GetCurrentAgencyIdAsync();
            var item = await _context.AgendaItems
                .Include(i => i.AgendaRecord)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.AgendaRecordId == recordId && i.AgendaRecord.AgencyId == agencyId);

            if (item != null)
            {
                _context.AgendaItems.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Satır silindi.";
            }

            return RedirectToAction(nameof(MeetingRoom), new { id = recordId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGridRow(int itemId, int recordId, string speaker, string statement, string? technicalNote)
        {
            var agencyId = await GetCurrentAgencyIdAsync();
            var item = await _context.AgendaItems
                .Include(i => i.AgendaRecord)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.AgendaRecordId == recordId && i.AgendaRecord.AgencyId == agencyId);

            if (item != null)
            {
                item.TopicTitle = speaker;
                item.PresentationText = statement;
                item.LiveMeetingNotes = technicalNote ?? "";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Satır güncellendi.";
            }

            return RedirectToAction(nameof(MeetingRoom), new { id = recordId });
        }

        public async Task<IActionResult> DeleteTopicDocument(int documentId, int recordId)
        {
            var agencyId = await GetCurrentAgencyIdAsync();
            var doc = await _context.DocumentArchives.FirstOrDefaultAsync(d => d.Id == documentId && d.AgencyId == agencyId && !d.IsDeleted);
            if (doc != null)
            {
                doc.IsDeleted = true;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Belge silindi.";
            }
            return RedirectToAction(nameof(MeetingRoom), new { id = recordId });
        }

    }
}
