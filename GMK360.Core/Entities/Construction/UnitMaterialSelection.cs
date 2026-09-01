using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities.Construction
{
    public class UnitMaterialSelection : BaseEntity
    {
        public int BuildingUnitId { get; set; }
        public BuildingUnit BuildingUnit { get; set; }

        public int ProjectMaterialCatalogId { get; set; }
        public ProjectMaterialCatalog ProjectMaterialCatalog { get; set; }

        public string SelectedByUserId { get; set; } // Seçimi yapan kat sahibi
        public ApplicationUser SelectedByUser { get; set; }

        public DateTime SelectionDate { get; set; }
        public bool IsApprovedByAdmin { get; set; } // Firma onayladı mı?
    }
}
