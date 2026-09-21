namespace GMK360.Core.Entities.Construction
{
    public enum ProjectStatus
    {
        On_Gorusme_Talep = 0,     // İlk temas, Kentsel Dönüşüm / Fizibilite Aşaması
        Projelendirme_Teklif = 1, // Aday Projeler
        Aktif_Santiye = 2,        // Aktif Şantiyeler
        Tamamlandi_Teslim = 3,    // Biten
        Satista_Topraktan = 4,    // Satışa Çıkanlar
        Anlasma_Yapildi = 5,      // Anlaşma Yapılanlar
        Anlasma_Olmayan_Iptal = 6 // Anlaşma Olmayan
    }
}
