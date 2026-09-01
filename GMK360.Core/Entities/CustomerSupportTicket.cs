using System;
using System.ComponentModel.DataAnnotations;

namespace GMK360.Core.Entities
{
    public class CustomerSupportTicket : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string SenderId { get; set; } = null!; // Telefon numarası veya E-posta adresi

        [MaxLength(50)]
        public string Channel { get; set; } = "WhatsApp"; // WhatsApp, Email, Web vb.

        [Required]
        public string OriginalMessage { get; set; } = null!; // Müşterinin yazdığı orijinal metin

        public string? AiResponse { get; set; } // AI'ın ürettiği cevap (Eğer otonom cevaplandıysa)

        public bool IsResolvedByAi { get; set; } = false; // AI tarafından mı çözüldü?

        [MaxLength(50)]
        public string TicketType { get; set; } = "SupportRequest";

        [MaxLength(50)]
        public string Status { get; set; } = "PendingAdmin"; // AutoResolved, PendingAdmin, Closed

        public string? ApplicationUserId { get; set; } 
    }
}
