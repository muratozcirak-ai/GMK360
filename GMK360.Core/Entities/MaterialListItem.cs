namespace GMK360.Core.Entities
{
    public class MaterialListItem : BaseEntity
    {
        public int MaterialListId { get; set; }
        public MaterialList MaterialList { get; set; }

        public string MaterialName { get; set; } // Örn: 20 Kg Saten Alçı
        public decimal Quantity { get; set; } // Örn: 3
        public string Unit { get; set; } // Örn: Adet, Kutu, Çuval
    }
}
