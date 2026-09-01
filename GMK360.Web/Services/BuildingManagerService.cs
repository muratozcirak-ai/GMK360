using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GMK360.Core.Interfaces;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;

namespace GMK360.Web.Services
{
    public class BuildingManagerService : IBuildingManagerService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public BuildingManagerService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task CalculateAndDistributeExpenseAsync(int expenseId)
        {
            var expense = await _context.BuildingExpenses.FindAsync(expenseId);
            if (expense == null) throw new Exception("Gider bulunamadı.");

            IQueryable<BuildingUnit> unitsQuery = _context.BuildingUnits.AsQueryable();

            if (expense.ScopeType == ExpenseScopeType.HousingComplex)
            {
                if (expense.HousingComplexId == null) throw new Exception("Site bilgisi eksik.");
                unitsQuery = unitsQuery.Where(u => u.Building.HousingComplexId == expense.HousingComplexId);
            }
            else if (expense.ScopeType == ExpenseScopeType.Building)
            {
                if (expense.BuildingId == null) throw new Exception("Bina bilgisi eksik.");
                unitsQuery = unitsQuery.Where(u => u.BuildingId == expense.BuildingId);
            }
            else if (expense.ScopeType == ExpenseScopeType.Unit)
            {
                // Daire bazlı giderler özel olarak atanmalıdır, toplu dağıtılmaz.
                throw new Exception("Daire bazlı giderler otomatik dağıtılamaz.");
            }

            var units = await unitsQuery.ToListAsync();
            
            if (!units.Any()) throw new Exception("Giderin dağıtılacağı aktif daire bulunamadı.");

            decimal dividedAmount = expense.TotalAmount / units.Count;

            foreach (var unit in units)
            {
                var debt = new UnitDebt
                {
                    BuildingUnitId = unit.Id,
                    BuildingExpenseId = expense.Id,
                    Amount = dividedAmount,
                    IsPaid = false
                };
                _context.UnitDebts.Add(debt);
            }

            await _context.SaveChangesAsync();
        }

        public async Task AddTenantToUnitAsync(int unitId, string tenantName, string tenantEmail, string tenantPhone)
        {
            var unit = await _context.BuildingUnits.FindAsync(unitId);
            if (unit == null) throw new Exception("Daire bulunamadı.");

            var existingUser = await _userManager.FindByEmailAsync(tenantEmail);
            ApplicationUser tenantUser;

            if (existingUser != null)
            {
                tenantUser = existingUser;
            }
            else
            {
                // Shadow Account oluşturuluyor
                tenantUser = new ApplicationUser
                {
                    UserName = tenantEmail,
                    Email = tenantEmail,
                    FirstName = tenantName,
                    PhoneNumber = tenantPhone,
                    IsShadowAccount = true,
                    UserType = UserType.Individual
                };

                var password = GenerateRandomPassword();
                var result = await _userManager.CreateAsync(tenantUser, password);

                if (!result.Succeeded)
                {
                    throw new Exception("Kullanıcı oluşturulurken bir hata oluştu.");
                }
                
                // Email/SMS ile shadow hesaba bildirim gönderilmesi
                string subject = "GMK360 Bina Yönetim Hesabınız Oluşturuldu";
                string body = $"<p>Merhaba {tenantName},</p><p>Yöneticiniz tarafından GMK360 Bina Yönetim sistemine kaydınız yapılmıştır.</p><p>Giriş e-posta adresiniz: <strong>{tenantEmail}</strong><br>Geçici Şifreniz: <strong>{password}</strong></p><p>Lütfen sisteme giriş yaptıktan sonra şifrenizi değiştirin.</p>";
                await _emailService.SendEmailAsync(tenantEmail, subject, body);
            }

            unit.TenantUserId = tenantUser.Id;
            unit.TenantName = tenantName;
            unit.TenantPhone = tenantPhone;
            unit.TenantEmail = tenantEmail;

            _context.BuildingUnits.Update(unit);
            await _context.SaveChangesAsync();
        }

        private string GenerateRandomPassword()
        {
            // Basit bir şifre üretici (Örnek amaçlı)
            return $"Temp!{Guid.NewGuid().ToString().Substring(0, 8)}1";
        }
    }
}
