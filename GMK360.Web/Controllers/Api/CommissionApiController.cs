using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace GMK360.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommissionApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CommissionApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("pay")]
        public async Task<IActionResult> PayCommission([FromBody] PayCommissionRequest request)
        {
            if (request == null || request.ReservationId <= 0)
            {
                return BadRequest("Geçersiz istek.");
            }

            var reservation = await _context.PropertyReservations
                .Include(r => r.Partner)
                .FirstOrDefaultAsync(r => r.Id == request.ReservationId);

            if (reservation == null)
            {
                return NotFound("Rezervasyon bulunamadı.");
            }

            if (reservation.IsCommissionPaid)
            {
                return BadRequest("Bu komisyon zaten ödenmiş.");
            }

            reservation.IsCommissionPaid = true;
            reservation.CommissionDocumentType = request.DocumentType;
            reservation.CommissionPaymentDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Komisyon ödemesi başarıyla kaydedildi." });
        }
    }

    public class PayCommissionRequest
    {
        public int ReservationId { get; set; }
        public string DocumentType { get; set; }
    }
}
