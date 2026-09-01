using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers
{
    [Route("export/xml")]
    public class ExportController : Controller
    {
        [HttpGet("{token}")]
        public async Task<IActionResult> GetXmlByToken(string token)
        {
            // TODO: Token kontrolü ve XML çıktısı oluşturma
            return Content("<sahibinden><ilanlar></ilanlar></sahibinden>", "application/xml");
        }
    }
}
