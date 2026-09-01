using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class EidsValidationLog : BaseEntity
    {
        public int? PropertyId { get; set; }
        public string ConsultantId { get; set; } = null!; // ApplicationUser Id is string (GUID by default in IdentityUser)

        public string RequestType { get; set; } = null!;
        public DateTime RequestDate { get; set; } = DateTime.Now;
        public int ResponseCode { get; set; }
        public string? ResponseMessage { get; set; }
        public bool IsSuccess { get; set; }
        public string? SystemReferenceId { get; set; }

        public virtual Property? Property { get; set; }
        public virtual ApplicationUser Consultant { get; set; } = null!;
    }
}
