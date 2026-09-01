using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers;

[Authorize]
public class WalletController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly GMK360.Core.Interfaces.IFinancialEngineService _financeEngine;

    public WalletController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, GMK360.Core.Interfaces.IFinancialEngineService financeEngine)
    {
        _db = db;
        _userManager = userManager;
        _financeEngine = financeEngine;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var transactions = await _db.UserWalletTransactions
            .Where(t => t.ApplicationUserId == user.Id)
            .OrderByDescending(t => t.CreatedAt)
            .Take(50)
            .ToListAsync();

        ViewBag.RealBalance = user.RealMoneyBalance;
        ViewBag.GiftBalance = user.GiftBalance;
        
        // Aktif hediyeleri kontrol et
        var activeGifts = await _db.WalletCredits
            .Where(w => w.UserId == user.Id && !w.IsUsed && w.ExpiryDate >= DateTime.UtcNow)
            .ToListAsync();
            
        ViewBag.ActiveGiftsCount = activeGifts.Count;

        var financeSettings = await _db.GlobalFinanceSettings.FirstOrDefaultAsync();
        ViewBag.MinimumWithdrawalAmount = financeSettings?.MinimumWithdrawalAmount ?? 10000m;
        
        return View(transactions);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TopUp(decimal amount)
    {
        if (amount <= 0)
        {
            TempData["Error"] = "Geçersiz yükleme tutarı.";
            return RedirectToAction("Index");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // 1. İşlem kaydı
        var txn = new UserWalletTransaction
        {
            ApplicationUserId = user.Id,
            Amount = amount,
            TransactionType = "TopUp",
            Description = $"Kredi kartı ile {amount:C2} bakiye yüklendi."
        };

        // 2. Bakiyeyi artır (Sadece gerçek para)
        user.RealMoneyBalance += amount;

        _db.UserWalletTransactions.Add(txn);
        await _userManager.UpdateAsync(user);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"{amount:C2} tutarında bakiye cüzdanınıza başarıyla yüklendi!";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestWithdrawal(decimal amount)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        try
        {
            await _financeEngine.RequestWithdrawalAsync(user.Id, amount);
            TempData["Success"] = $"{amount:C2} tutarındaki nakit çekim talebiniz başarıyla alındı. Finans ekibimiz inceleyip onaylayacaktır.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Index");
    }
}
