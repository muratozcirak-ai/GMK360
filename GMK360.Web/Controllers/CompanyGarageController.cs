using GMK360.Core.Entities.Logistics;
using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class CompanyGarageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public CompanyGarageController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        
        [HttpGet]
        public async Task<IActionResult> AllTasks(string? date, string? category, int? vehicleId)
        {
            var agencyId = await GetUserAgencyIdAsync() ?? 1;
            ViewBag.Projects = await _context.ConstructionProjects.Where(p => !p.IsDeleted).Select(p => new { p.Id, p.Name }).ToListAsync();
            ViewBag.Vehicles = await _context.CompanyVehicles.Where(v => v.AgencyId == agencyId && v.Status == "Aktif").ToListAsync();
            
            var query = _context.VehicleTasks
                .Include(t => t.Vehicle)
                .Where(t => t.AgencyId == agencyId && !t.IsDeleted);

            if (!string.IsNullOrEmpty(date))
            {
                var parsedDate = DateTime.Parse(date).Date;
                query = query.Where(t => t.TaskDate.Date == parsedDate);
                ViewBag.SelectedDate = parsedDate.ToString("yyyy-MM-dd");
            }
            else
            {
                ViewBag.SelectedDate = "";
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(t => t.TaskCategory == category);
                ViewBag.SelectedCategory = category;
            }

            if (vehicleId.HasValue)
            {
                query = query.Where(t => t.VehicleId == vehicleId.Value);
                ViewBag.SelectedVehicleId = vehicleId.Value;
            }

            var tasks = await query.OrderByDescending(t => t.TaskDate).ThenByDescending(t => t.Id).ToListAsync();
            return View(tasks);
        }

        [HttpGet("CompanyGarage/AllExpenses")]
        public async Task<IActionResult> AllExpenses(int? vehicleId, string expenseType, DateTime? startDate, DateTime? endDate)
        {
            var agencyId = await GetUserAgencyIdAsync() ?? 1;
            var query = _context.VehicleExpenses.Include(e => e.Vehicle).Where(e => e.Vehicle.AgencyId == agencyId);
            
            if (vehicleId.HasValue) query = query.Where(e => e.VehicleId == vehicleId);
            if (!string.IsNullOrEmpty(expenseType)) query = query.Where(e => e.ExpenseType == expenseType);
            if (startDate.HasValue) query = query.Where(e => e.ExpenseDate >= startDate);
            if (endDate.HasValue) query = query.Where(e => e.ExpenseDate <= endDate);

            ViewBag.Vehicles = await _context.CompanyVehicles.Where(v => v.AgencyId == agencyId).ToListAsync();
            ViewBag.SelectedVehicleId = vehicleId;
            ViewBag.SelectedType = expenseType;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            var expenses = await query.OrderByDescending(e => e.ExpenseDate).ToListAsync();
            return View(expenses);
        }

        [HttpPost("CompanyGarage/StartTask")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> StartTask(int taskId, int vehicleId, string driverName, int startKm)
        {
            var task = await _context.VehicleTasks.FindAsync(taskId);
            if (task != null)
            {
                task.VehicleId = vehicleId;
                task.DriverName = driverName;
                task.StartKm = startKm;
                task.Status = "Yolda / Görevde";
                
                var vehicle = await _context.CompanyVehicles.FindAsync(vehicleId);
                if(vehicle != null && startKm > vehicle.CurrentKm) {
                    vehicle.CurrentKm = startKm;
                }
                
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("AllTasks");
        }

        [HttpGet]

        public async Task<IActionResult> VehicleExpenses(int id)
        {
            var vehicle = await _context.CompanyVehicles
                .Include(v => v.Expenses)
                .FirstOrDefaultAsync(v => v.Id == id);
                
            if (vehicle == null) return NotFound();
            
            return View(vehicle);
        }

        private async Task<int?> GetUserAgencyIdAsync()
        {
            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin")) return 1;
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            var consultant = await _context.AgencyConsultants.FirstOrDefaultAsync(c => c.UserId == user.Id && c.IsActive);
            return consultant?.AgencyId;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.Projects = await _context.ConstructionProjects.Where(p => !p.IsDeleted).Select(p => new { p.Id, p.Name }).ToListAsync();
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var vehicles = await _context.CompanyVehicles
                .Include(v => v.Assignments)
                .Include(v => v.Tasks)
                .Include(v => v.Expenses)
                .Where(v => v.AgencyId == agencyId.Value && v.Status == "Aktif")
                .ToListAsync();

            ViewBag.ActiveProjects = await _context.ConstructionProjects
                .Where(p => p.AgencyId == agencyId.Value)
                .Select(p => new { p.Id, p.Name })
                .ToListAsync();

            return View(vehicles);
        }

        [HttpPost]
        public async Task<IActionResult> AddVehicle(
            string plateNumber, string type, string brand, int? initialKm, decimal? initialHours, string? assignedTo,
            string? registrationSerialNo, IFormFile? registrationPhoto,
            string? insuranceCompany, string? insurancePolicyNo, DateTime? insuranceExpiryDate, IFormFile? insurancePhoto,
            DateTime? inspectionExpiryDate)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var vehicle = new CompanyVehicle
            {
                AgencyId = agencyId.Value,
                PlateNumber = plateNumber?.Trim().ToUpper() ?? "PLAKASIZ",
                VehicleType = type,
                BrandModel = brand,
                CurrentKm = initialKm ?? 0,
                CurrentWorkingHours = initialHours ?? 0,
                RegistrationSerialNo = registrationSerialNo?.Trim().ToUpper(),
                InsuranceCompany = insuranceCompany?.Trim(),
                InsurancePolicyNo = insurancePolicyNo?.Trim(),
                InsuranceExpiryDate = insuranceExpiryDate,
                InspectionExpiryDate = inspectionExpiryDate,
                Status = "Aktif"
            };

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "garage_docs");
            Directory.CreateDirectory(uploadsFolder);

            if (registrationPhoto != null && registrationPhoto.Length > 0)
            {
                var ext = Path.GetExtension(registrationPhoto.FileName);
                var fileName = $"ruhsat_{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await registrationPhoto.CopyToAsync(stream);
                }
                vehicle.RegistrationPhotoPath = "/uploads/garage_docs/" + fileName;
            }

            if (insurancePhoto != null && insurancePhoto.Length > 0)
            {
                var ext = Path.GetExtension(insurancePhoto.FileName);
                var fileName = $"police_{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await insurancePhoto.CopyToAsync(stream);
                }
                vehicle.InsurancePolicyPhotoPath = "/uploads/garage_docs/" + fileName;
            }

            _context.CompanyVehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(assignedTo))
            {
                var assign = new VehicleAssignment
                {
                    AgencyId = agencyId.Value,
                    VehicleId = vehicle.Id,
                    AssignedToName = assignedTo.Trim(),
                    AssignmentDate = DateTime.UtcNow
                };
                _context.VehicleAssignments.Add(assign);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"{vehicle.PlateNumber} ruhsat ve poliçe bilgileriyle birlikte araç parkına başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateVehicleDocuments(
            int vehicleId,
            string? registrationSerialNo, IFormFile? registrationPhoto,
            string? insuranceCompany, string? insurancePolicyNo, DateTime? insuranceExpiryDate, IFormFile? insurancePhoto,
            DateTime? inspectionExpiryDate)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var vehicle = await _context.CompanyVehicles.FirstOrDefaultAsync(v => v.Id == vehicleId && v.AgencyId == agencyId.Value);
            if (vehicle == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(registrationSerialNo))
                vehicle.RegistrationSerialNo = registrationSerialNo.Trim().ToUpper();

            if (!string.IsNullOrWhiteSpace(insuranceCompany))
                vehicle.InsuranceCompany = insuranceCompany.Trim();

            if (!string.IsNullOrWhiteSpace(insurancePolicyNo))
                vehicle.InsurancePolicyNo = insurancePolicyNo.Trim();

            if (insuranceExpiryDate.HasValue)
                vehicle.InsuranceExpiryDate = insuranceExpiryDate;

            if (inspectionExpiryDate.HasValue)
                vehicle.InspectionExpiryDate = inspectionExpiryDate;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "garage_docs");
            Directory.CreateDirectory(uploadsFolder);

            if (registrationPhoto != null && registrationPhoto.Length > 0)
            {
                var ext = Path.GetExtension(registrationPhoto.FileName);
                var fileName = $"ruhsat_{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await registrationPhoto.CopyToAsync(stream);
                }
                vehicle.RegistrationPhotoPath = "/uploads/garage_docs/" + fileName;
            }

            if (insurancePhoto != null && insurancePhoto.Length > 0)
            {
                var ext = Path.GetExtension(insurancePhoto.FileName);
                var fileName = $"police_{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await insurancePhoto.CopyToAsync(stream);
                }
                vehicle.InsurancePolicyPhotoPath = "/uploads/garage_docs/" + fileName;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"{vehicle.PlateNumber} ruhsat ve poliçe belgeleri başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddSampleFleet()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var sampleVehicles = new List<CompanyVehicle>
            {
                new CompanyVehicle
                {
                    AgencyId = agencyId.Value,
                    PlateNumber = "34 HFR 401",
                    VehicleType = "Damperli Hafriyat Kamyonu",
                    BrandModel = "Mercedes Axor 3340 (40 Ayak)",
                    CurrentKm = 142800,
                    CurrentWorkingHours = 0,
                    RegistrationSerialNo = "AN 894512",
                    InsuranceCompany = "Allianz Sigorta",
                    InsurancePolicyNo = "POL-3340-9842",
                    InsuranceExpiryDate = DateTime.Today.AddMonths(8),
                    InspectionExpiryDate = DateTime.Today.AddMonths(4),
                    Status = "Aktif"
                },
                new CompanyVehicle
                {
                    AgencyId = agencyId.Value,
                    PlateNumber = "İŞ-01 (YIKIM)",
                    VehicleType = "Ekskavatör (Paletli Yıkım & Kırıcı)",
                    BrandModel = "CAT 336 D2L (Yıkım Kırıcılı)",
                    CurrentKm = 0,
                    CurrentWorkingHours = 3420,
                    RegistrationSerialNo = "MAK-CAT-336",
                    InsuranceCompany = "Anadolu Sigorta (İş Makinası Kasko)",
                    InsurancePolicyNo = "KSK-CAT-7712",
                    InsuranceExpiryDate = DateTime.Today.AddMonths(10),
                    Status = "Aktif"
                },
                new CompanyVehicle
                {
                    AgencyId = agencyId.Value,
                    PlateNumber = "İŞ-02 (KEPÇE)",
                    VehicleType = "Beko Loder (Kepçe / Kazıcı)",
                    BrandModel = "JCB 4CX Turbo Sitemaster",
                    CurrentKm = 0,
                    CurrentWorkingHours = 1850,
                    RegistrationSerialNo = "MAK-JCB-4CX",
                    InsuranceCompany = "Türkiye Sigorta",
                    InsurancePolicyNo = "KSK-JCB-5541",
                    InsuranceExpiryDate = DateTime.Today.AddMonths(6),
                    Status = "Aktif"
                },
                new CompanyVehicle
                {
                    AgencyId = agencyId.Value,
                    PlateNumber = "34 ŞNT 88",
                    VehicleType = "Şantiye Pikabı (4x4)",
                    BrandModel = "Toyota Hilux 2.4 D-4D Çift Kabin",
                    CurrentKm = 64500,
                    CurrentWorkingHours = 0,
                    RegistrationSerialNo = "BB 410294",
                    InsuranceCompany = "Axa Sigorta",
                    InsurancePolicyNo = "TRF-90214-AX",
                    InsuranceExpiryDate = DateTime.Today.AddMonths(11),
                    InspectionExpiryDate = DateTime.Today.AddMonths(5),
                    Status = "Aktif"
                },
                new CompanyVehicle
                {
                    AgencyId = agencyId.Value,
                    PlateNumber = "34 TANK 12",
                    VehicleType = "Su Tankeri (Arazöz - Toz Bastırma)",
                    BrandModel = "Ford Cargo 1833 Arazöz",
                    CurrentKm = 98200,
                    CurrentWorkingHours = 450,
                    RegistrationSerialNo = "CC 773190",
                    InsuranceCompany = "Sompo Sigorta",
                    InsurancePolicyNo = "POL-TNK-1209",
                    InsuranceExpiryDate = DateTime.Today.AddMonths(7),
                    InspectionExpiryDate = DateTime.Today.AddMonths(3),
                    Status = "Aktif"
                }
            };

            _context.CompanyVehicles.AddRange(sampleVehicles);
            await _context.SaveChangesAsync();

            // Örnek ilk görev, döküm seferleri ve iş makinası puantajları
            var truck = sampleVehicles[0];
            var exc = sampleVehicles[1];
            var jcb = sampleVehicles[2];

            _context.VehicleTasks.AddRange(
                // 1. Kamyon Döküm Seferleri (Çetele)
                new VehicleTask
                {
                    AgencyId = agencyId.Value,
                    VehicleId = truck.Id,
                    DriverName = "Kemal Usta (Damper Şoförü)",
                    TaskCategory = "Döküm Seferi",
                    DumpSiteName = "İBB Cebeci Döküm Sahası",
                    MaterialType = "Yıkım Molozu",
                    TripNumber = 1,
                    TicketNumber = "KNT-88190",
                    Tonnage = 24.5m,
                    StartTime = "09:15",
                    EndTime = "11:00",
                    TaskDate = DateTime.Today,
                    Status = "Tamamlandı",
                    TaskDescription = "[Döküm Seferi #1] Cebeci Sahası - Yıkım Molozu (24.5 Ton) - Fiş: KNT-88190"
                },
                new VehicleTask
                {
                    AgencyId = agencyId.Value,
                    VehicleId = truck.Id,
                    DriverName = "Kemal Usta (Damper Şoförü)",
                    TaskCategory = "Döküm Seferi",
                    DumpSiteName = "İBB Cebeci Döküm Sahası",
                    MaterialType = "Yıkım Molozu",
                    TripNumber = 2,
                    TicketNumber = "KNT-88235",
                    Tonnage = 26.2m,
                    StartTime = "11:30",
                    EndTime = "13:15",
                    TaskDate = DateTime.Today,
                    Status = "Tamamlandı",
                    TaskDescription = "[Döküm Seferi #2] Cebeci Sahası - Yıkım Molozu (26.2 Ton) - Fiş: KNT-88235"
                },
                new VehicleTask
                {
                    AgencyId = agencyId.Value,
                    VehicleId = truck.Id,
                    DriverName = "Kemal Usta (Damper Şoförü)",
                    TaskCategory = "Döküm Seferi",
                    DumpSiteName = "İBB Cebeci Döküm Sahası",
                    MaterialType = "Yıkım Molozu",
                    TripNumber = 3,
                    TicketNumber = "KNT-88310",
                    Tonnage = 23.8m,
                    StartTime = "14:00",
                    EndTime = "15:45",
                    TaskDate = DateTime.Today,
                    Status = "Tamamlandı",
                    TaskDescription = "[Döküm Seferi #3] Cebeci Sahası - Yıkım Molozu (23.8 Ton) - Fiş: KNT-88310"
                },
                new VehicleTask
                {
                    AgencyId = agencyId.Value,
                    VehicleId = truck.Id,
                    DriverName = "Kemal Usta (Damper Şoförü)",
                    TaskCategory = "Döküm Seferi",
                    DumpSiteName = "İBB Cebeci Döküm Sahası",
                    MaterialType = "Yıkım Molozu",
                    TripNumber = 4,
                    TicketNumber = "KNT-88402",
                    Tonnage = 25.0m,
                    StartTime = "16:15",
                    TaskDate = DateTime.Today,
                    Status = "Yolda / Görevde",
                    TaskDescription = "[Döküm Seferi #4] Cebeci Sahası - Yıkım Molozu (25.0 Ton) - Döküm Yolunda"
                },
                // 2. İş Makinası Sayaç Puantajları
                new VehicleTask
                {
                    AgencyId = agencyId.Value,
                    VehicleId = exc.Id,
                    DriverName = "Murat Operatör",
                    TaskCategory = "Makine Puantajı",
                    StartWorkHour = 3412.0m,
                    EndWorkHour = 3420.0m,
                    WorkingHours = 8.0m,
                    FuelLiters = 135.0m,
                    TaskDate = DateTime.Today,
                    StartTime = "08:30",
                    EndTime = "17:30",
                    Status = "Tamamlandı",
                    TaskDescription = "[Makine Puantajı] A Blok 3. Kat Kolon/Kiriş Makaslı Kırımı ve Kamyon Yükleme (8.0 Saat, 135 Lt Mazot)"
                },
                new VehicleTask
                {
                    AgencyId = agencyId.Value,
                    VehicleId = jcb.Id,
                    DriverName = "Ali Operatör",
                    TaskCategory = "Makine Puantajı",
                    StartWorkHour = 1843.5m,
                    EndWorkHour = 1850.0m,
                    WorkingHours = 6.5m,
                    FuelLiters = 60.0m,
                    TaskDate = DateTime.Today,
                    StartTime = "09:00",
                    EndTime = "16:30",
                    Status = "Tamamlandı",
                    TaskDescription = "[Makine Puantajı] Şantiye Çevre Temizliği, Moloz Toplama ve İstifleme (6.5 Saat, 60 Lt Mazot)"
                }
            );

            // Örnek döküm ve yakıt masrafı
            _context.VehicleExpenses.AddRange(
                new VehicleExpense
                {
                    AgencyId = agencyId.Value,
                    VehicleId = truck.Id,
                    ExpenseType = "Döküm Sahası Harcı (Moloz / Kazı)",
                    Amount = 1850,
                    ReceiptNumber = "İBB-DKM-98421",
                    ReportedBy = "Kemal Usta",
                    ExpenseDate = DateTime.Today,
                    OdometerAtExpense = truck.CurrentKm
                },
                new VehicleExpense
                {
                    AgencyId = agencyId.Value,
                    VehicleId = exc.Id,
                    ExpenseType = "Yakıt (Şantiye Tankeri Mazot)",
                    Amount = 6400,
                    ReceiptNumber = "MZT-45210",
                    ReportedBy = "Murat Operatör",
                    ExpenseDate = DateTime.Today
                }
            );

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "🎉 Harika! Örnek Yıkım & Hafriyat Filosu (5 Araç, Döküm Seferleri, Kantar Fişleri ve Makine Puantajları) başarıyla yüklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AssignVehicle(int vehicleId, string assignedTo, int? projectId, string notes)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var activeAssign = await _context.VehicleAssignments
                .FirstOrDefaultAsync(a => a.VehicleId == vehicleId && a.ReturnDate == null);
            
            if (activeAssign != null) activeAssign.ReturnDate = DateTime.UtcNow;

            var newAssign = new VehicleAssignment
            {
                AgencyId = agencyId.Value,
                VehicleId = vehicleId,
                AssignedToName = assignedTo,
                ProjectId = projectId,
                Notes = notes,
                AssignmentDate = DateTime.UtcNow
            };

            _context.VehicleAssignments.Add(newAssign);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Araç başarıyla zimmetlendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(int vehicleId, string driverName, int? projectId, string description, string taskDate, string startTime, string endTime, string? taskCategory, string? dumpSiteName, int? tripCount)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var vehicle = await _context.CompanyVehicles.FindAsync(vehicleId);

            var prefix = !string.IsNullOrWhiteSpace(taskCategory) ? $"[{taskCategory}] " : "";
            var dumpInfo = !string.IsNullOrWhiteSpace(dumpSiteName) ? $" (Döküm: {dumpSiteName} - {tripCount ?? 1} Sefer)" : "";
            var finalDesc = $"{prefix}{description}{dumpInfo}";

            var task = new VehicleTask
            {
                AgencyId = agencyId.Value,
                VehicleId = vehicleId,
                DriverName = driverName,
                DestinationProjectId = projectId,
                TaskDescription = finalDesc,
                TaskDate = string.IsNullOrEmpty(taskDate) ? DateTime.UtcNow : DateTime.Parse(taskDate),
                Status = "Bekliyor",
                StartKm = vehicle?.CurrentKm,
                StartTime = startTime,
                EndTime = endTime
            };

            _context.VehicleTasks.Add(task);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Araç Görevi / Seferi başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTaskStatus(int taskId, string status, int? endKm, decimal? workingHours)
        {
            var task = await _context.VehicleTasks.Include(t => t.Vehicle).FirstOrDefaultAsync(t => t.Id == taskId);
            if(task != null) {
                task.Status = status;
                
                if (endKm.HasValue && endKm.Value > (task.StartKm ?? 0))
                {
                    task.EndKm = endKm;
                    if (task.Vehicle != null) {
                        task.Vehicle.CurrentKm = endKm.Value;
                    }
                }
                
                if (workingHours.HasValue && workingHours.Value > 0)
                {
                    task.WorkingHours = workingHours;
                    if (task.Vehicle != null) {
                        task.Vehicle.CurrentWorkingHours += workingHours.Value;
                    }
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddExpense(int vehicleId, string expenseType, decimal amount, string receiptNumber, string expenseDate, int? odometer, Microsoft.AspNetCore.Http.IFormFile? photo)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var exp = new VehicleExpense
            {
                AgencyId = agencyId.Value,
                VehicleId = vehicleId,
                ExpenseType = expenseType,
                Amount = amount,
                ReceiptNumber = receiptNumber,
                ReportedBy = _userManager.GetUserName(User),
                ExpenseDate = string.IsNullOrEmpty(expenseDate) ? DateTime.UtcNow : DateTime.Parse(expenseDate),
                OdometerAtExpense = odometer
            };
            
            if (photo != null && photo.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(photo.FileName);
                var uploadsFolder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads", "garage");
                System.IO.Directory.CreateDirectory(uploadsFolder);
                var filePath = System.IO.Path.Combine(uploadsFolder, fileName);
                using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }
                exp.PhotoPath = "/uploads/garage/" + fileName;
            }
            
            // Eğer girilen KM, aracın mevcut KM'sinden büyükse aracın güncel KM'sini de otomatik güncelle
            if (odometer.HasValue)
            {
                var vehicle = await _context.CompanyVehicles.FindAsync(vehicleId);
                if (vehicle != null && odometer.Value > vehicle.CurrentKm)
                {
                    vehicle.CurrentKm = odometer.Value;
                }
            }

            _context.VehicleExpenses.Add(exp);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Masraf/Fiş başarıyla araca işlendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddDumpTrip(
            int vehicleId, int? projectId, string driverName, string dumpSiteName, string materialType,
            int? tripNumber, string ticketNumber, decimal? tonnage, string taskDate, string? startTime, Microsoft.AspNetCore.Http.IFormFile? ticketPhoto)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var vehicle = await _context.CompanyVehicles.FindAsync(vehicleId);

            var date = string.IsNullOrEmpty(taskDate) ? DateTime.Today : DateTime.Parse(taskDate).Date;
            if (!tripNumber.HasValue || tripNumber.Value <= 0)
            {
                var existingTripsToday = await _context.VehicleTasks
                    .CountAsync(t => t.VehicleId == vehicleId && t.TaskDate.Date == date && t.TaskCategory == "Döküm Seferi");
                tripNumber = existingTripsToday + 1;
            }

            var task = new VehicleTask
            {
                AgencyId = agencyId.Value,
                VehicleId = vehicleId,
                DestinationProjectId = projectId,
                DriverName = driverName,
                TaskCategory = "Döküm Seferi",
                DumpSiteName = dumpSiteName,
                MaterialType = materialType,
                TripNumber = tripNumber,
                TicketNumber = ticketNumber,
                Tonnage = tonnage,
                TaskDate = date,
                StartTime = startTime ?? DateTime.Now.ToString("HH:mm"),
                Status = "Tamamlandı",
                TaskDescription = $"[Döküm Seferi #{tripNumber}] {dumpSiteName} - {materialType} ({tonnage?.ToString("N1") ?? "-"} Ton) - Fiş: {ticketNumber ?? "-"}"
            };

            if (ticketPhoto != null && ticketPhoto.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tickets");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"kantar_{Guid.NewGuid()}{Path.GetExtension(ticketPhoto.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ticketPhoto.CopyToAsync(stream);
                }
                task.TicketPhotoPath = "/uploads/tickets/" + fileName;
            }

            _context.VehicleTasks.Add(task);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"🚚 {vehicle?.PlateNumber} için {tripNumber}. Döküm Seferi ({dumpSiteName} - {tonnage} Ton) başarıyla kaydedildi!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddMachineryWorkLog(
            int vehicleId, int? projectId, string operatorName, decimal startWorkHour, decimal endWorkHour,
            string description, decimal? fuelLiters, string logDate, Microsoft.AspNetCore.Http.IFormFile? counterPhoto)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var vehicle = await _context.CompanyVehicles.FindAsync(vehicleId);
            if (vehicle == null) return NotFound();

            var netHours = endWorkHour >= startWorkHour ? (endWorkHour - startWorkHour) : 0;
            var date = string.IsNullOrEmpty(logDate) ? DateTime.Today : DateTime.Parse(logDate).Date;

            var task = new VehicleTask
            {
                AgencyId = agencyId.Value,
                VehicleId = vehicleId,
                DestinationProjectId = projectId,
                DriverName = operatorName,
                TaskCategory = "Makine Puantajı",
                StartWorkHour = startWorkHour,
                EndWorkHour = endWorkHour,
                WorkingHours = netHours,
                FuelLiters = fuelLiters,
                TaskDate = date,
                Status = "Tamamlandı",
                TaskDescription = $"[Makine Puantajı] {description} ({netHours:N1} Saat) - Sayaç: {startWorkHour:N1} -> {endWorkHour:N1}"
            };

            if (endWorkHour > vehicle.CurrentWorkingHours)
            {
                vehicle.CurrentWorkingHours = endWorkHour;
            }

            if (counterPhoto != null && counterPhoto.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "machinery");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"sayac_{Guid.NewGuid()}{Path.GetExtension(counterPhoto.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await counterPhoto.CopyToAsync(stream);
                }
                task.TicketPhotoPath = "/uploads/machinery/" + fileName;
            }

            _context.VehicleTasks.Add(task);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"🚜 {vehicle.PlateNumber} için {netHours:N1} saatlik çalışma puantajı başarıyla işlendi! (Güncel Sayaç: {vehicle.CurrentWorkingHours:N1} Saat)";
            return RedirectToAction(nameof(Index));
        }
    }
}
