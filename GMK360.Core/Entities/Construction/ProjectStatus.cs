namespace GMK360.Core.Entities.Construction
{
    public enum ProjectStatus
    {
        Projelendirme_Teklif = 1, // Henüz anlaşılmamış, üzerinde çalışılan
        Aktif_Santiye = 2,        // Anlaşılmış, çarkların döndüğü güncel şantiye
        Tamamlandi_Teslim = 3,    // Biten, referans olarak web sitesine konacaklar
        Satista_Topraktan = 4     // Satış ofisi için açık olan / topraktan satış
    }
}
