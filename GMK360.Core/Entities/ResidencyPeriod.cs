using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    /// <summary>
    /// Bir bağımsız bölümde aynı anda en fazla 1 aktif Mal Sahibi ve 1 aktif Kiracı dönemi tutulur.
    /// Taşınma/satışta eski dönem ARCHIVED olur ve borç/makbuz geçmişi yeni sakine karışmaz.
    /// </summary>
    public class ResidencyPeriod : BaseEntity
    {
        public int BuildingUnitId { get; set; }
        [ForeignKey("BuildingUnitId")]
        public virtual BuildingUnit BuildingUnit { get; set; } = null!;

        [MaxLength(450)]
        public string? ResidentUserId { get; set; }
        [ForeignKey("ResidentUserId")]
        public virtual ApplicationUser? ResidentUser { get; set; }

        [Required, MaxLength(20)]
        public string RoleType { get; set; } = "Owner"; // "Owner" | "Tenant"

        [Required, MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(40)]
        public string? Phone { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "ACTIVE"; // "ACTIVE" | "ARCHIVED"

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }

        public DateTime? ArchivedAt { get; set; }
        [MaxLength(450)]
        public string? ArchivedByUserId { get; set; }
        [MaxLength(500)]
        public string? ArchiveNote { get; set; }

        // Oylama & Onay PIN hash'i (kişiye özel) ve dijital vekalet bilgileri
        [MaxLength(256)]
        public string? VotingPinHash { get; set; }
        [MaxLength(450)]
        public string? ProxyUserId { get; set; }
        [MaxLength(500)]
        public string? ProxyDocumentUrl { get; set; }

        public virtual ICollection<UnitDebt> Debts { get; set; } = new List<UnitDebt>();
    }
}
