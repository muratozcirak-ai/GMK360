namespace GMK360.Core.Entities
{
    public enum BuildingManagerRole
    {
        HeadManager = 1,    // Yönetim Kurulu Başkanı / Ana Yönetici (İçeriden veya dışarıdan)
        BoardMember = 2,    // Yönetim Kurulu Üyesi / Aza
        Auditor = 3,        // Denetçi / Denetim Kurulu Üyesi (KMK Md. 41: Kat maliki zorunlu)
        ViceChair = 4,      // Başkan Yardımcısı
        ReserveMember = 5   // Yedek Üye
    }

    public enum SignatureAuthorityType
    {
        None = 0,   // İmza Yetkisi Yok (Üye/Aza)
        Single = 1, // Tek Başına İmzaya Yetkili
        Joint = 2   // Müşterek (Çift İmzalı) Yetkili
    }
}
