using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities.Construction
{
    public enum ChecklistCategoryType
    {
        TechnicalQuality = 1, // Teknik ve Kalite Kontrol (Paspayı, Beton vb.)
        OHS = 2,              // İSG - İş Sağlığı ve Güvenliği (Asansör boşluğu, bantlar vb.)
        Administrative = 3,   // İdari ve Resmi Evrak Kontrolü
        Handover = 4          // Müşteriye Teslimat Öncesi Son Kontrol
    }

    // 1. KURUMLAR İÇİN ŞABLON (Template)
    public class ChecklistTemplate : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } // Örn: "Beton Döküm Öncesi Kontrol" veya "Kat İSG Kontrolü"
        
        public string? Description { get; set; }
        
        [MaxLength(100)]
        public string? TargetPhase { get; set; } // Örn: "Faz 3", "İnce İşçilik"

        public ChecklistCategoryType Category { get; set; } = ChecklistCategoryType.TechnicalQuality;
        
        public virtual ICollection<ChecklistTemplateItem> Items { get; set; } = new List<ChecklistTemplateItem>();
    }

    public class ChecklistTemplateItem : BaseEntity
    {
        public int ChecklistTemplateId { get; set; }
        public virtual ChecklistTemplate ChecklistTemplate { get; set; }

        [Required]
        public string QuestionText { get; set; } // Örn: "Demirler arasına paspayı konuldu mu?" veya "Asansör boşluğuna uyarı bandı çekildi mi?"
        
        public bool IsPhotoRequired { get; set; } = false; // Hukuki koruma için zorunlu fotoğraf
        public int OrderIndex { get; set; }
    }

    // 2. SAHA UYGULAMASI (Instance) - Göreve/Faza atanan liste
    public class TaskChecklist : BaseEntity
    {
        [Required]
        public int ConstructionTaskId { get; set; }
        public virtual ConstructionTask ConstructionTask { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } 

        public ChecklistCategoryType Category { get; set; } = ChecklistCategoryType.TechnicalQuality;

        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedAt { get; set; }

        public virtual ICollection<TaskChecklistItem> Items { get; set; } = new List<TaskChecklistItem>();
    }

    public class TaskChecklistItem : BaseEntity
    {
        public int TaskChecklistId { get; set; }
        public virtual TaskChecklist TaskChecklist { get; set; }

        [Required]
        public string QuestionText { get; set; } 

        public bool IsPhotoRequired { get; set; } = false;
        
        public bool IsChecked { get; set; } = false; // Mühendis/İSG Uzmanı Tik attı mı?
        
        public string? Note { get; set; } 
        public string? ProofPhotoUrl { get; set; } // Sahadan çekilen anlık kanıt fotoğrafı

        public string? CheckedByUserId { get; set; } // Tıklayan Mühendis veya İSG Uzmanı
        [ForeignKey("CheckedByUserId")]
        public virtual ApplicationUser CheckedByUser { get; set; }

        public DateTime? CheckedAt { get; set; }
    }
}
