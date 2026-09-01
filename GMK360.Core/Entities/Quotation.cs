namespace GMK360.Core.Entities
{
    public class Quotation : BaseEntity
    {
        public int RenovationRequestId { get; set; } // Hangi talep için teklif verildi
        public RenovationRequest RenovationRequest { get; set; }

        public string TradesmanUserId { get; set; } // Teklifi veren usta
        public Identity.ApplicationUser TradesmanUser { get; set; }

        public decimal LaborCost { get; set; }
        public decimal MaterialCost { get; set; }
        
        public decimal TotalCost => LaborCost + MaterialCost;

        public bool IsAccepted { get; set; } = false; // Teklif kabul edildi mi?
    }
}
