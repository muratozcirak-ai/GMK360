using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Core.Interfaces;
using GMK360.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Services
{
    public class BuildingManagementService : IBuildingManagementService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public BuildingManagementService(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<List<Building>> GetManagerBuildingsAsync(string managerUserId)
        {
            return await _context.Buildings
                .Where(b => b.ManagerUserId == managerUserId)
                .Include(b => b.Units)
                .ToListAsync();
        }

        public async Task<Building> GetBuildingByIdAsync(int id)
        {
            return await _context.Buildings
                .Include(b => b.Units)
                .Include(b => b.Expenses)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Building> CreateBuildingAsync(Building building)
        {
            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();
            return building;
        }

        public async Task<BuildingUnit> AddUnitAsync(BuildingUnit unit)
        {
            _context.BuildingUnits.Add(unit);
            await _context.SaveChangesAsync();
            return unit;
        }

        public async Task<BuildingExpense> AddExpenseAsync(BuildingExpense expense, List<int> unitIds)
        {
            _context.BuildingExpenses.Add(expense);
            await _context.SaveChangesAsync(); // To get Expense ID

            // If unitIds provided, split the debt
            if (unitIds != null && unitIds.Any())
            {
                var amountPerUnit = expense.TotalAmount / unitIds.Count;
                foreach (var unitId in unitIds)
                {
                    var debt = new UnitDebt
                    {
                        BuildingUnitId = unitId,
                        BuildingExpenseId = expense.Id,
                        Amount = amountPerUnit,
                        IsPaid = false
                    };
                    _context.UnitDebts.Add(debt);
                }
                await _context.SaveChangesAsync();
            }

            return expense;
        }

        public async Task<BuildingAnnouncement> AddAnnouncementAsync(BuildingAnnouncement announcement)
        {
            _context.Set<BuildingAnnouncement>().Add(announcement);
            await _context.SaveChangesAsync();
            return announcement;
        }

        public async Task<UnitDebt> MarkDebtAsPaidAsync(int debtId, string paidBy)
        {
            var debt = await _context.UnitDebts
                .Include(d => d.BuildingExpense)
                .Include(d => d.BuildingUnit)
                .FirstOrDefaultAsync(d => d.Id == debtId);

            if (debt != null)
            {
                debt.IsPaid = true;
                debt.PaymentDate = DateTime.Now;
                debt.PaidBy = paidBy;
                await _context.SaveChangesAsync();

                // Generate Digital Receipt / Email
                var subject = $"Dijital Makbuz: {debt.BuildingExpense.Description} Ödemeniz Alındı";
                var body = $"Sayın {debt.BuildingUnit.OwnerName} / {debt.BuildingUnit.TenantName},\n\n" +
                           $"Binanızdaki {debt.BuildingUnit.UnitNumber} numaralı dairenize ait '{debt.BuildingExpense.Description}' için {debt.Amount} TL ödemeniz {debt.PaymentDate} tarihinde alınmıştır.\n\n" +
                           $"Ödeyen: {debt.PaidBy}\n\nTeşekkür ederiz.";

                // Attempt to notify resident (tenant first, then owner)
                var emailTo = !string.IsNullOrEmpty(debt.BuildingUnit.TenantEmail) 
                    ? debt.BuildingUnit.TenantEmail 
                    : debt.BuildingUnit.OwnerEmail;

                if (!string.IsNullOrEmpty(emailTo))
                {
                    await _emailService.SendEmailAsync(emailTo, subject, body);
                }
            }

            return debt;
        }

        public async Task<List<BuildingUnit>> GetResidentUnitsAsync(string email)
        {
            return await _context.BuildingUnits
                .Include(u => u.Building)
                .Where(u => u.OwnerEmail == email || u.TenantEmail == email)
                .ToListAsync();
        }

        public async Task<BuildingUnit> GetResidentUnitByIdAsync(int unitId, string email)
        {
            return await _context.BuildingUnits
                .Include(u => u.Building)
                .FirstOrDefaultAsync(u => u.Id == unitId && (u.OwnerEmail == email || u.TenantEmail == email));
        }

        public async Task<List<BuildingAnnouncement>> GetBuildingAnnouncementsAsync(int buildingId)
        {
            return await _context.Set<BuildingAnnouncement>()
                .Where(a => a.BuildingId == buildingId && !a.IsDeleted)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<List<UnitDebt>> GetUnitDebtsAsync(int unitId)
        {
            return await _context.UnitDebts
                .Include(d => d.BuildingExpense)
                .Where(d => d.BuildingUnitId == unitId)
                .OrderByDescending(d => d.Id)
                .ToListAsync();
        }
    }
}
