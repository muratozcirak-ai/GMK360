using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers;

[Authorize]
public class CampaignController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public CampaignController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // Davet ettiği kişileri bul
        var referredUsers = await _userManager.Users
            .Where(u => u.ReferredByUserId == user.Id)
            .Select(u => new 
            {
                u.FirstName,
                u.LastName
            })
            .ToListAsync();

        // Bu referanslardan kazanılan toplam net tutarı hesapla
        var totalBonus = await _db.UserWalletTransactions
            .Where(t => t.ApplicationUserId == user.Id && t.TransactionType == "ReferralBonus")
            .SumAsync(t => t.Amount);

        ViewBag.ReferralCode = user.ReferralCode;
        ViewBag.TotalBonus = totalBonus;
        ViewBag.ReferredCount = referredUsers.Count;
        
        // Gizlilik gereği anonim listeleme yapalım (Sadece İlk harfler)
        var networkList = referredUsers.Select(u => new 
        {
            Name = $"{(string.IsNullOrEmpty(u.FirstName) ? "A" : u.FirstName.Substring(0, 1))}*** {(string.IsNullOrEmpty(u.LastName) ? "B" : u.LastName.Substring(0, 1))}***",
            Date = DateTime.UtcNow.AddDays(-1) // Geçici tarih
        }).ToList();

        ViewBag.NetworkList = networkList;

        return View();
    }
}
