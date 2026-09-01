using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class UtilitySubscription : BaseEntity
    {
        public int PropertyId { get; set; }
        public virtual Property Property { get; set; } = null!;

        public int LiabilityTypeId { get; set; }
        public virtual LiabilityType LiabilityType { get; set; } = null!;

        public int? InstitutionId { get; set; } // Sistemde tanımlıysa
        public virtual Institution? Institution { get; set; }

        public string? CustomInstitutionName { get; set; } // Sistemde yoksa kullanıcının girdiği ad

        public string? SubscriberNo { get; set; }
        public string? SubscriberName { get; set; }
        
        public int? ExpectedBillDay { get; set; } // 1-31 arası
        public bool UseIyzico { get; set; } // Iyzico/Otomatik ödeme talimatı
        
        public bool IsActive { get; set; } = true;
    }
}
