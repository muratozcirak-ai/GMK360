using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers
{
    public class CommissionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommissionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Bekleyen komisyon ödemelerini getir
            var pendingCommissions = await _context.PropertyReservations
                .Include(r => r.Partner)
                .Include(r => r.Property)
                .Where(r => r.PartnerId != null && !r.IsCommissionPaid && r.CommissionAmount > 0)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(pendingCommissions);
        }
    }
}
