using System;

namespace GMK360.Core.Entities
{
    public class GlobalFinanceSettings : BaseEntity
    {
        public decimal DefaultWithholdingTaxRate { get; set; } = 20.0m; // %20
        public int EscrowHoldDays { get; set; } = 14;
        public decimal MinimumWithdrawalAmount { get; set; } = 10000.0m; // 10,000 TL
    }
}
