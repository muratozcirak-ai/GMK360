using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Core.Entities;
using GMK360.Data.Contexts;

namespace GMK360.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class AdminFinanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminFinanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _context.GlobalFinanceSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new GlobalFinanceSettings();
                _context.GlobalFinanceSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            var funds = await _context.SystemFunds.ToListAsync();
            
            ViewBag.Funds = funds;
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSettings(GlobalFinanceSettings model)
        {
            var settings = await _context.GlobalFinanceSettings.FirstOrDefaultAsync();
            if (settings != null)
            {
                settings.DefaultWithholdingTaxRate = model.DefaultWithholdingTaxRate;
                settings.EscrowHoldDays = model.EscrowHoldDays;
                settings.MinimumWithdrawalAmount = model.MinimumWithdrawalAmount;
                
                _context.GlobalFinanceSettings.Update(settings);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Finansal ayarlar başarıyla güncellendi.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFund(string name, decimal percentage)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var fund = new SystemFund
                {
                    Name = name,
                    Percentage = percentage,
                    IsActive = true,
                    Balance = 0
                };
                
                _context.SystemFunds.Add(fund);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Yeni fon başarıyla eklendi.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFund(int id)
        {
            var fund = await _context.SystemFunds.FindAsync(id);
            if (fund != null)
            {
                fund.IsActive = !fund.IsActive;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Escrows()
        {
            var escrows = await _context.EscrowTransactions
                .Include(e => e.SellerUser)
                .Include(e => e.ReferrerUser)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return View(escrows);
        }

        // --- DİNAMİK VERGİ VE YÜKÜMLÜLÜK MOTORU (FINANCIAL OBLIGATION TYPES) ---

        public async Task<IActionResult> ObligationTypes()
        {
            try
            {
                var types = await _context.FinancialObligationTypes
                    .OrderBy(f => f.Name)
                    .ToListAsync();
                return View(types);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Vergi türleri yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveObligationType(FinancialObligationType model)
        {
            if (model.Id == 0)
            {
                model.CreatedAt = DateTime.Now;
                _context.FinancialObligationTypes.Add(model);
                TempData["SuccessMessage"] = "Yeni vergi/yükümlülük türü başarıyla eklendi.";
            }
            else
            {
                var existing = await _context.FinancialObligationTypes.FindAsync(model.Id);
                if (existing != null)
                {
                    existing.Name = model.Name;
                    existing.TargetPropertyType = model.TargetPropertyType;
                    existing.ResponsibleRole = model.ResponsibleRole;
                    existing.PaymentFrequency = model.PaymentFrequency;
                    existing.FirstInstallmentMonth = model.FirstInstallmentMonth;
                    existing.SecondInstallmentMonth = model.SecondInstallmentMonth;
                    existing.UpdatedAt = DateTime.Now;
                    
                    _context.FinancialObligationTypes.Update(existing);
                    TempData["SuccessMessage"] = "Kayıt güncellendi.";
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ObligationTypes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleObligationType(int id)
        {
            var item = await _context.FinancialObligationTypes.FindAsync(id);
            if (item != null)
            {
                item.IsActive = !item.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Durum güncellendi.";
            }
            return RedirectToAction(nameof(ObligationTypes));
        }
    }
}
