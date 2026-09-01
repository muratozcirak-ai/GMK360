using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GMK360.Core.Entities;
using GMK360.Core.Interfaces;

namespace GMK360.Core.Services
{
    public class BuildingExpenseService : IBuildingExpenseService
    {
        private readonly DbContext _context;

        public BuildingExpenseService(DbContext context)
        {
            _context = context;
        }

        public async Task PayExpenseShareAsync(int expenseShareId, string paidByUserId)
        {
            var share = await _context.Set<BuildingExpenseShare>()
                .Include(s => s.BuildingExpense)
                .Include(s => s.Property)
                .FirstOrDefaultAsync(s => s.Id == expenseShareId);

            if (share == null) throw new Exception("Expense Share not found.");
            if (share.Status == ExpenseShareStatus.Paid) throw new Exception("This share is already paid.");

            share.Status = ExpenseShareStatus.Paid;
            share.PaidDate = DateTime.Now;
            share.PaidByUserId = paidByUserId;

            // Demirbaş / Yatırım (Capital Expenditure) MANTIKLARI
            if (share.BuildingExpense.ExpenseType == BuildingExpenseType.CapitalExpenditure)
            {
                // 1. KİRACI ÖDEDİYSE (KİRADAN DÜŞ)
                // Eğer işlemi yapan (paidByUserId) mülk sahibi DEĞİLSE (Yani kiracıysa)
                if (share.Property.OwnerUserId != paidByUserId)
                {
                    // Bir sonraki ayın kirasından otomatik mahsuplaşma için negatif takvim veya mevcut takvim indirimi yapılabilir.
                    var nextRent = await _context.Set<PropertyFinancialSchedule>()
                        .Where(s => s.PropertyId == share.PropertyId && s.UserId == paidByUserId && s.Status == ScheduleStatus.Pending)
                        .OrderBy(s => s.DueDate)
                        .FirstOrDefaultAsync();

                    if (nextRent != null)
                    {
                        // Kirayı bu tutar kadar eksilt
                        nextRent.Amount -= share.ShareAmount;
                        // Not: Eğer share > rent ise sonraki aylara devreden bir bakiye (kredi) mantığı kurulmalıdır.
                    }
                }

                // 2. VERGİ İNDİRİMİ İÇİN PROPERTY EXPENSES TABLOSUNA EKLE (Ev Sahibine)
                var propertyExpense = new PropertyExpense
                {
                    PropertyId = share.PropertyId,
                    ExpenseCategory = PropertyExpenseCategory.MaintenanceRepair,
                    Amount = share.ShareAmount,
                    ExpenseDate = DateTime.Now,
                    InvoiceDocumentUrl = share.BuildingExpense.InvoiceDocumentUrl, // Asıl faturanın URL'si
                    IsTaxDeductible = true,
                    TaxYear = DateTime.Now.Year
                };
                
                _context.Add(propertyExpense);
            }

            // Aidat (Operational) ise ekstra bir vergi yansıması olmaz (Çünkü aidatı kiracı öder, vergiye konu değildir).

            await _context.SaveChangesAsync();
        }
    }
}
