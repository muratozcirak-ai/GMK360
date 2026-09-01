using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GMK360.Core.Entities;
using GMK360.Core.Interfaces;

namespace GMK360.Core.Services
{
    // Note: Assuming ApplicationDbContext or similar exists for DI, we use a generic DbContext here for demonstration
    public class ScheduleGeneratorService : IScheduleGeneratorService
    {
        private readonly DbContext _context;

        public ScheduleGeneratorService(DbContext context)
        {
            _context = context;
        }

        public async Task GenerateSchedulesForPropertyAsync(int propertyId, int taxYear)
        {
            var property = await _context.Set<Property>()
                .Include(p => p.Type) // Konut mu İşyeri mi anlamak için
                .FirstOrDefaultAsync(p => p.Id == propertyId);

            if (property == null)
                throw new Exception("Property not found");

            // 1. Mülkün Konut (Residential) mu İşyeri (Commercial) mi olduğunu belirle
            // Gerçek projede DefinitionValue veya Enum üzerinden kontrol edilir.
            bool isResidential = property.Type?.Name?.Contains("Konut", StringComparison.OrdinalIgnoreCase) ?? true;
            bool isCommercial = property.Type?.Name?.Contains("İşyeri", StringComparison.OrdinalIgnoreCase) ?? false;

            TargetPropertyType targetType = isResidential ? TargetPropertyType.OnlyResidential : TargetPropertyType.OnlyCommercial;

            // 2. Aktif olan Yasal Yükümlülük Kurallarını Çek
            var activeRules = await _context.Set<FinancialObligationType>()
                .Where(r => r.IsActive && 
                           (r.TargetPropertyType == TargetPropertyType.All || r.TargetPropertyType == targetType))
                .ToListAsync();

            foreach (var rule in activeRules)
            {
                // Mevcut yılda bu kural için zaten schedule eklenmiş mi kontrol et
                var existingSchedule = await _context.Set<PropertyFinancialSchedule>()
                    .AnyAsync(s => s.PropertyId == propertyId && 
                                   s.ObligationTypeId == rule.Id && 
                                   s.DueDate.Year == taxYear);

                if (existingSchedule)
                    continue; // Zaten eklenmiş

                string responsibleUserId = rule.ResponsibleRole == ResponsibleRole.Owner 
                                            ? property.OwnerUserId 
                                            : property.UserId; // Kiracı veya Danışman

                if (string.IsNullOrEmpty(responsibleUserId))
                    continue; // Sorumlu atanmamışsa takvime ekleme

                decimal estimatedAmount = 0; // Gerçekte rayiç bedel veya kira üzerinden hesaplanacak
                
                // 1. Taksit Ekleme
                if (rule.FirstInstallmentMonth.HasValue)
                {
                    var schedule1 = new PropertyFinancialSchedule
                    {
                        PropertyId = property.Id,
                        UserId = responsibleUserId,
                        ObligationTypeId = rule.Id,
                        Amount = estimatedAmount,
                        DueDate = new DateTime(taxYear, rule.FirstInstallmentMonth.Value, DateTime.DaysInMonth(taxYear, rule.FirstInstallmentMonth.Value)),
                        Status = ScheduleStatus.Pending
                    };
                    _context.Add(schedule1);
                }

                // 2. Taksit Ekleme
                if (rule.SecondInstallmentMonth.HasValue)
                {
                    var schedule2 = new PropertyFinancialSchedule
                    {
                        PropertyId = property.Id,
                        UserId = responsibleUserId,
                        ObligationTypeId = rule.Id,
                        Amount = estimatedAmount,
                        DueDate = new DateTime(taxYear, rule.SecondInstallmentMonth.Value, DateTime.DaysInMonth(taxYear, rule.SecondInstallmentMonth.Value)),
                        Status = ScheduleStatus.Pending
                    };
                    _context.Add(schedule2);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
