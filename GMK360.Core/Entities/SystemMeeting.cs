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
        public bool IsSiteWide { get; set; } = false;
        public int? HousingComplexId { get; set; }

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
        public string DecisionCategory { get; set; } = "General"; // General, DuesUpdate, AppointManager, AppointBoardMember, CapitalProject, ManagementPlan

        public string? ProposedPayloadJson { get; set; }

        public bool AutoExecuteOnQuorum { get; set; } = true;

        public bool IsExecuted { get; set; } = false;

        public DateTime? ExecutedAt { get; set; }

        [MaxLength(500)]
        public string? ExecutionSummary { get; set; }

        [MaxLength(40)]
        public string? VerificationToken { get; set; } // QR Kod doğrulama kodu

        public bool IsConcluded { get; set; } = false; // Toplantı tamamlandı mı?

        [MaxLength(32)]
        public string WorkflowStage { get; set; } = "Decision"; // Discussion (Ön Görüşme/Toplantı), Decision (Karar Defteri), ArchivedNegative (Olumsuz Kapatıldı)

        [MaxLength(64)]
        public string? InvitationScope { get; set; } // OwnersAndProxyTenants, OwnersOnly, AllResidents, BoardOnly

        [MaxLength(500)]
        public string? ClosingNote { get; set; }

        public int? BookPageNumber { get; set; } // Resmi Noter Defteri Sayfa No

        public DateTime? SourceDocDate { get; set; } // Karara esas olan evrakın tarihi

        [MaxLength(60)]
        public string? SourceDocNumber { get; set; } // Karara esas olan evrakın numarası (Örn: TPL-2026/01)

        [MaxLength(200)]
        public string? SourceDocSender { get; set; } // Karara esas olan evrakın nereden gönderildiği

        [MaxLength(150)]
        public string? ChairpersonName { get; set; } // Toplantı / Kurul Başkanı adı soyadı

        [MaxLength(500)]
        public string? BoardMembersSummary { get; set; } // Azaların adı soyadı

        [MaxLength(20)]
        public string? MeetingEndTime { get; set; } // Örn: 21:15'te sona erdi

        // Toplantıya bağlı kararlar
        public virtual ICollection<SystemMeetingDecision> Decisions { get; set; } = new List<SystemMeetingDecision>();

        // Uzaktan ön onay oyları ve vekalet kayıtları
        public virtual ICollection<BuildingDecisionVote> Votes { get; set; } = new List<BuildingDecisionVote>();

        // Yönetim kurulu asil üye (Seviye 1-3) PIN oyları
        public virtual ICollection<BoardDecisionVote> BoardVotes { get; set; } = new List<BoardDecisionVote>();

        // Uzaktan eklenen teklif, keşif belgesi ve görüşler
        public virtual ICollection<BuildingDecisionAttachment> Attachments { get; set; } = new List<BuildingDecisionAttachment>();
        
        // Toplantıya bağlı anketler (Survey/Poll)
        public virtual ICollection<MeetingSurvey> Surveys { get; set; } = new List<MeetingSurvey>();
    }
}
