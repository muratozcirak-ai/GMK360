using Microsoft.AspNetCore.Mvc;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Construction.Workflows;
using System.Linq;

namespace GMK360.Web.Controllers
{
    public class WorkGroupController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WorkGroupController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var data = _context.WorkGroupDefinitions.Where(w => !w.IsDeleted).ToList();
            return View(data);
        }

        [HttpPost]
        public IActionResult Create(WorkGroupDefinition model)
        {
            model.IsActive = true;
            model.CreatedAt = System.DateTime.Now;
            _context.WorkGroupDefinitions.Add(model);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
