namespace GMK360.Core.Entities
{
    public enum ListingState
    {
        Active = 1,      // Yayında / Aktif İlan
        Passive = 2,     // Pasif (Kullanıcı dondurmuş)
        Sold = 3,        // Satıldı
        Rented = 4,      // Kiralandı
        Cancelled = 5,   // Satıştan/Kiralamadan Vazgeçildi
        Draft = 6,       // Taslak İlan (Henüz yayına girmemiş)
        Revoked = 7,     // Yetkisi Alındı (Sistem Tarafından İptal Edilen/Sahte İlan)
        Expired = 8,     // Süresi Doldu (Yetki Belgesi Süresi Biten İlan)
        PrivateTracking = 9, // İlan Değil - Özel Mülk Takibi (Dijital Evim)
        PendingApproval = 10 // Yönetici onayı bekliyor
    }
}
