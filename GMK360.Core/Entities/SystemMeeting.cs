using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class SystemMeeting : BaseEntity
    {
        // Generic Context Fields (örneğin: "Building", "Agency")
        public string ContextType { get; set; } = "Building";
        public int ContextId { get; set; }

        [Required]
        public DateTime MeetingDate { get; set; }

        [Required]
        [MaxLength(300)]
        public string AgendaTitle { get; set; } // Örn: Yeni Yıl Bütçe Görüşmesi ve Çatı Yalıtımı

        [MaxLength(40)]
        public string? DecisionNumber { get; set; } // Örn: 2026/01

        [MaxLength(200)]
        public string? MeetingLocation { get; set; } // Örn: Apartman Girişi / Yönetim Odası

        [MaxLength(30)]
        public string QuorumType { get; set; } = "Standard"; // Standard (>50%), Innovation (KMK42 >50%), ManagementPlan (80%), Unanimous (100%)

        [MaxLength(40)]
        public string? VerificationToken { get; set; } // QR Kod doğrulama kodu

        public bool IsConcluded { get; set; } = false; // Toplantı tamamlandı mı?

        // Toplantıya bağlı kararlar
        public virtual ICollection<SystemMeetingDecision> Decisions { get; set; } = new List<SystemMeetingDecision>();

        // Uzaktan ön onay oyları ve vekalet kayıtları
        public virtual ICollection<BuildingDecisionVote> Votes { get; set; } = new List<BuildingDecisionVote>();

        // Uzaktan eklenen teklif, keşif belgesi ve görüşler
        public virtual ICollection<BuildingDecisionAttachment> Attachments { get; set; } = new List<BuildingDecisionAttachment>();
        
        // Toplantıya bağlı anketler (Survey/Poll)
        public virtual ICollection<MeetingSurvey> Surveys { get; set; } = new List<MeetingSurvey>();
    }
}
