using GMK360.Core.Entities;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class MasterMaterial : BaseEntity
    {
        public string MaterialCode { get; set; } // Örn: "MAT-YTONG-20", "MAT-DMR-16"
        public string MaterialName { get; set; } // Örn: "20'lik Ytong Blok", "Ø16 Nervürlü Demir"
        public string Category { get; set; }     // Örn: "Kaba_Demir", "Ince_Duvar", "Yalitim"
        public string BaseUnit { get; set; }     // Örn: "Adet", "Ton", "Kg", "m2"
    }
}
