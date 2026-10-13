namespace GMK360.Core.DTOs.Construction
{
    public class DxfTakeoffDto
    {
        public int ElevationId { get; set; } // Hangi katın çizimi?
        public string MaterialCode { get; set; } // DXF Layer'ı (Örn: "MAT-YTONG-20")
        public decimal Quantity { get; set; } // Çizimden çıkan net metraj (Örn: 500 m2)
    }
}
