using System;

namespace GMK360.Core.DTOs.Construction
{
    public class ProjectRentPolicyDto
    {
        // 🏠 Daire Politikası
        public decimal FlatRelocationFee { get; set; } = 40000m;
        public decimal FlatMonthlyRent { get; set; } = 30000m;
        public DateTime? FlatStartDate { get; set; }
        public DateTime? FlatEndDate { get; set; }
        public int FlatPaymentDay { get; set; } = 5;
        public int FlatGraceDays { get; set; } = 3;

        // 🏢 Dükkan Politikası
        public decimal ShopRelocationFee { get; set; } = 50000m;
        public decimal ShopMonthlyRent { get; set; } = 45000m;
        public DateTime? ShopStartDate { get; set; }
        public DateTime? ShopEndDate { get; set; }
        public int ShopPaymentDay { get; set; } = 5;
        public int ShopGraceDays { get; set; } = 3;
    }
}
