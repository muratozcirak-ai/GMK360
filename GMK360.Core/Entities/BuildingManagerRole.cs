namespace GMK360.Core.Entities
{
    public enum BuildingManagerRole
    {
        HeadManager = 1,  // Ana Yönetici (Dışarıdan veya içeriden tam yetkili)
        BoardMember = 2,  // Yönetim Kurulu Üyesi / Aza (Kararlara katılır)
        Auditor = 3       // Denetçi (Kasa hareketlerini ve kararları denetler)
    }
}
