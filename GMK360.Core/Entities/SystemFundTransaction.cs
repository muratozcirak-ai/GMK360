using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class SystemFundTransaction : BaseEntity
    {
        [Required]
        [ForeignKey("SystemFund")]
        public int SystemFundId { get; set; }
        public virtual SystemFund SystemFund { get; set; } = null!;

        public int? SourceEscrowTransactionId { get; set; }
        public virtual EscrowTransaction? SourceEscrowTransaction { get; set; }

        public decimal Amount { get; set; }
    }
}
