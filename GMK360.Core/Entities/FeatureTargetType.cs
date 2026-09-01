namespace GMK360.Core.Entities
{
    public enum FeatureTargetType
    {
        Both = 0,     // Hem Siteye hem Daireye eklenebilir (veya belirsiz)
        Property = 1, // Sadece Daire/İç Özellikler (Ebeveyn Banyosu, Balkon)
        Complex = 2   // Sadece Site/Bina Özellikleri (Tenis Kortu, Açık Havuz)
    }
}
