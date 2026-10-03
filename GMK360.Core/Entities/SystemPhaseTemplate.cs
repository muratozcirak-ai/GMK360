using GMK360.Core.Entities.Construction;

namespace GMK360.Core.Entities
{
    public class SystemPhaseTemplate : BaseEntity
    {
        public BudgetPhaseCategory PhaseCategory { get; set; }
        public string SubCategory { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string? ItemCode { get; set; } // Poz Kodu / İmalat Kodu
        public bool IsQuoteRequired { get; set; } = false;
    }
}