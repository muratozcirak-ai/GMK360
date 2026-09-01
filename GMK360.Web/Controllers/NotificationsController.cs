using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;

namespace GMK360.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public NotificationsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var logs = await _db.SystemNotificationLogs
                .OrderByDescending(x => x.SentAt)
                .Take(100)
                .ToListAsync();

            return View(logs);
        }
    }
}
