using Microsoft.AspNetCore.Identity;

namespace GMK360.Core.Entities.Identity
{
    public enum UserType
    {
        Individual,       // Bireysel Kullanıcı (Ev arayan)
        PropertyOwner,    // Mülk Sahibi / Yatırımcı (Mülk yönetimi, sözleşme, gider takibi)
        Corporate,        // Kurumsal
        Consultant,       // Emlak Danışmanı
        ServiceProvider,  // Usta / Hizmet Veren
        Supplier,         // Nalbur ve Tedarikçiler
        CommercialRenter  // Ticari Kiralayan (Günlük Kiralık vb.)
    }

    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? DisplayName { get; set; }
        public string? ProfileSlug { get; set; }
        public string? TcIdentityNo { get; set; }
        public int? BirthYear { get; set; }
        
        public UserType UserType { get; set; }
        
        // Kullanıcının seçtiği modüller (Virgülle ayrılmış liste, örn: "construction,realestate,facility")
        public string? ActiveModules { get; set; }

        public string? RecoveryQuestion { get; set; }

        public bool HasMapConsent { get; set; } = false;
        public System.DateTime? MapConsentDate { get; set; }

        public string? RecoveryAnswer { get; set; }
        
        public bool IsEDevletVerified { get; set; } = false;
        
        // Apartman Yöneticisi tarafından (veya sistem tarafından) otomatik oluşturulup henüz şifre belirlenmemiş/aktive edilmemiş hesaplar
        public bool IsShadowAccount { get; set; } = false;
        
        // Turizm İşletmecileri İçin KBS (Kriptolanacak)
        public string? KbsFacilityCode { get; set; }
        public string? KbsPassword { get; set; }
        
        public string? ProfileImageUrl { get; set; }

        // --- GÜVENLİK & DİJİTAL İMZA (İŞLEM ŞİFRESİ) ---
        public string? TransactionPinHash { get; set; } // Oylama, toplu SMS, para transferi gibi kritik işlemler için 6 haneli 2. onay şifresi (Hash'lenmiş)
        public bool IsTransactionPinSet { get; set; } = false;

        // --- CÜZDAN & REFERANS SİSTEMİ ---
        public decimal RealMoneyBalance { get; set; } = 0;
        public decimal GiftBalance { get; set; } = 0;
        
        // Emlak Danışmanları / AI Kullanımı için Jeton-Kredi Bakiyesi
        public int WalletBalance { get; set; } = 0;

        public string? ReferralCode { get; set; } // Bu kullanıcının başkalarını davet edeceği token/kod
        public string? ReferredByUserId { get; set; } // Bu kullanıcıyı kim davet etti? (Komisyon için)
        
        // --- KAMPANYA VE VAAT YÖNETİMİ ---
        // Kullanıcı üye olduğunda hangi kampanyadan geldiğini "mühürler".
        // 10 gün sonra kampanya değişse bile bu kullanıcının hakkı (vaat edilen) asla kaybolmaz.
        public int? RegisteredCampaignId { get; set; } 
        
        public ICollection<AgencyConsultant> AgencyConsultants { get; set; }
        public virtual ICollection<EidsValidationLog> EidsLogs { get; set; } = new List<EidsValidationLog>();
        
        public string? ShadowCreatorId { get; set; } // Bu hesabı oluşturan gerçek yöneticinin UserId'si

        // Kullanıcının yönetici, denetçi veya aza olduğu binalar
        
        public virtual ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();
        public virtual ICollection<BuildingManager> ManagedBuildings { get; set; } = new List<BuildingManager>();
    }
}
