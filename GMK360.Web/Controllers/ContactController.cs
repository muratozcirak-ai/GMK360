using GMK360.Core.Entities;
using GMK360.Core.Interfaces;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers
{
    public class ContactController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly ApplicationDbContext _context;

        public ContactController(IEmailService emailService, ApplicationDbContext context)
        {
            _emailService = emailService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Send(string name, string email, string phone, string subject, string message)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(message))
            {
                TempData["ErrorMessage"] = "Lütfen zorunlu alanlarý doldurun.";
                return RedirectToAction("Index");
            }

            // 1. Veritabanina Kaydet
            var contactMessage = new ContactMessage
            {
                Name = name,
                Email = email,
                Phone = phone ?? "",
                Subject = subject ?? "Konu Belirtilmedi",
                Message = message
            };
            
            _context.ContactMessages.Add(contactMessage);
            await _context.SaveChangesAsync();

            // 2. Email Gonder
            string mailBody = $@"
            <h3>Yeni Ýletiþim Formu Mesajý</h3>
            <p><strong>Ad Soyad:</strong> {name}</p>
            <p><strong>E-Posta:</strong> {email}</p>
            <p><strong>Telefon:</strong> {phone}</p>
            <p><strong>Konu:</strong> {subject}</p>
            <p><strong>Mesaj:</strong><br/>{message.Replace("\n", "<br/>")}</p>
            ";

            // Hata firlatirsa diye try-catch, form gönderimi basarisiz olmasin
            try {
                await _emailService.SendEmailAsync("info@gmk360.com", $"Siteden Yeni Mesaj: {subject}", mailBody);
            } catch { }

            TempData["SuccessMessage"] = "Mesajýnýz baþarýyla alýnmýþtýr. En kýsa sürede size dönüþ yapacaðýz.";
            return RedirectToAction("Index");
        }
    }
}
