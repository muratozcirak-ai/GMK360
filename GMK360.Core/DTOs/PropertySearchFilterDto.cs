using System;
using System.Collections.Generic;

namespace GMK360.Core.DTOs
{
    public class PropertySearchFilterDto
    {
        public int? CityId { get; set; }
        public int? DistrictId { get; set; }
        public int? PropertyTypeId { get; set; } // Satılık, Kiralık, Turistik Kiralık

        // EAV Filtreleri (Dynamic Features)
        // Kullanıcının seçtiği Özellik Değerleri (Örn: Havuzlu, Eşyalı vb. DefinitionValue ID'leri)
        public List<int> FeatureIds { get; set; } = new List<int>();

        // Fiyat Aralığı
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        
        // Turistik Kiralama için Tarih Filtresi (Anti-Overlap)
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
    }
}
