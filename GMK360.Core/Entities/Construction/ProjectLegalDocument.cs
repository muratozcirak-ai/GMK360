using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities.Construction
{
    public class ProjectLegalDocument : BaseEntity
    {
        public int ConstructionProjectId { get; set; }
        public ConstructionProject ConstructionProject { get; set; }

        public int? SystemTemplateId { get; set; }
        public SystemLegalDocumentTemplate SystemTemplate { get; set; }

        public string DocumentName { get; set; }
        public string? Stage { get; set; }
        
        // ENUM İPTAL EDİLDİ - Düz String
        public string Status { get; set; } = "Bekliyor"; 
        
        public string? AssignedUserId { get; set; } 
        public string? InstitutionContact { get; set; } 
        
        public DateTime? StartDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        
        public decimal? EstimatedCost { get; set; } 
        public decimal? ActualCost { get; set; } 
        
        public string? IssueNotes { get; set; }

        public decimal? DocumentFee { get; set; }
        public decimal? AdditionalCost { get; set; }

        public int? TargetInstitutionId { get; set; }
        public InstitutionRecord TargetInstitution { get; set; }

        public int? TargetContactId { get; set; }
        public InstitutionContact TargetContact { get; set; } 
        public string? FilePath { get; set; } 
        public bool IsCustom { get; set; } 
        public int DisplayOrder { get; set; } // YENİ: Sıralama Kolonu

        [NotMapped]
        public bool IsLocked { get; set; }
        
        [NotMapped]
        public string? MissingPrerequisitesMessage { get; set; }
    }
}
