using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using System.Security.Claims;
using System;

namespace GMK360.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemIssueApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SystemIssueApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class IssueDto
        {
            public string IssueType { get; set; }
            public string Description { get; set; }
            public int? CityId { get; set; }
            public int? DistrictId { get; set; }
            public int? NeighborhoodId { get; set; }
            public string ContextData { get; set; }
        }

        [HttpPost("Report")]
        public async Task<IActionResult> ReportIssue([FromBody] IssueDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.IssueType))
            {
                return BadRequest("Invalid issue data.");
            }

            var ticket = new SystemIssueTicket
            {
                IssueType = dto.IssueType,
                Description = dto.Description,
                CityId = dto.CityId,
                DistrictId = dto.DistrictId,
                NeighborhoodId = dto.NeighborhoodId,
                ContextData = dto.ContextData,
                ReportedByUserId = User.Identity.IsAuthenticated ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null,
                CreatedAt = DateTime.UtcNow
            };

            _context.SystemIssueTickets.Add(ticket);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, ticketId = ticket.Id });
        }
    }
}
