namespace GMK360.Core.Entities.Enums
{
    public enum OccupantType
    {
        PayingTenant = 1,       // Normal Kiracı
        OwnerOccupied = 2,      // Mülk Sahibi Kendisi Oturuyor
        ExemptRelative = 3,     // Anne, Baba, Çocuk, Kardeş, Torun, Büyükanne/Büyükbaba - Ücretsiz (Muaf)
        NonExemptResident = 4   // Diğer Akraba/Arkadaş - Ücretsiz (Emsal Kira Bedeli)
    }
}
