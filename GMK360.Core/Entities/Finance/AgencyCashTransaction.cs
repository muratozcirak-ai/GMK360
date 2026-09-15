using System;

namespace GMK360.Core.Entities.Finance
{
    public enum AgencyCashTransactionType
    {
        SupplierPayment,    // Tedarikçi Ödemesi
        WorkerAdvance,      // Mavi Yaka Avans/Ödeme
        ConsultantAdvance,  // Beyaz Yaka Avans/Maaş Ödemesi
        GeneralExpense      // Genel Kasa Çıkışı
    }

    public class AgencyCashTransaction : BaseEntity
    {
        public int AgencyId { get; set; }
        public Agency Agency { get; set; }

        public AgencyCashTransactionType TransactionType { get; set; }

        // İlişkiler
        public int? SupplierCurrentAccountId { get; set; }
        public SupplierCurrentAccount? SupplierCurrentAccount { get; set; }

        public int? AgencyWorkerId { get; set; }
        public GMK360.Core.Entities.Construction.AgencyWorker? AgencyWorker { get; set; }

        public int? AgencyConsultantId { get; set; }
        public AgencyConsultant? AgencyConsultant { get; set; }

        // Ödeme Detayları
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Completed;
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        // Vade ve Çek/Kart Bilgileri
        public DateTime? DueDate { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? BankName { get; set; }

        public string Description { get; set; }
        public string? HandledByUserId { get; set; }
    }
}
