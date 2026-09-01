using System;
using GMK360.Core.Entities;

namespace GMK360.Core.DTOs
{
    public class FaturaGonderimDTO
    {
        public string AliciUnvan { get; set; }
        public string AliciVKN_TCKN { get; set; }
        public string AliciAdres { get; set; }
        public decimal ToplamTutar { get; set; }
        public decimal KdvTutari { get; set; }
        
        // E-Fatura veya E-Arşiv olma durumunu VKN uzunluğu veya sistemdeki kaydına göre 
        // servise göndermeden önce belirleyebiliriz.
        public FaturaTipi FaturaTipi { get; set; } 
        
        public string Aciklama { get; set; } // Örn: Emlak Portalı Hizmet Bedeli
    }
}
