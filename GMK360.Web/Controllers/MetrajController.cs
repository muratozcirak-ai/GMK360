using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using netDxf;
using netDxf.Entities;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class MetrajController : Controller
    {
        [HttpGet]
        public IActionResult Index(int projectId)
        {
            ViewData["ProjectId"] = projectId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ParseDxf(int projectId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Lütfen bir DXF dosyası yükleyin.");

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".dxf")
                return BadRequest("Sadece .dxf uzantılı dosyalar desteklenmektedir.");

            var tempPath = Path.GetTempFileName();
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            try
            {
                var doc = DxfDocument.Load(tempPath);
                if (doc == null)
                    return BadRequest("Dosya geçerli bir DXF formatında değil veya bozuk.");

                var blocks = doc.Entities.Inserts
                    .GroupBy(i => i.Block.Name)
                    .Select(g => new { 
                        BlockName = g.Key, 
                        Count = g.Count() 
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                // Pass results back to a view
                ViewData["ProjectId"] = projectId;
                return View("Result", blocks);
            }
            catch (System.Exception ex)
            {
                return BadRequest("Metraj çıkartılırken hata oluştu: " + ex.Message);
            }
            finally
            {
                if (System.IO.File.Exists(tempPath))
                    System.IO.File.Delete(tempPath);
            }
        }
    }
}
