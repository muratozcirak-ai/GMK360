using System;
using System.ComponentModel.DataAnnotations;

namespace GMK360.Core.Entities
{
    public class BolgeEkspertizHafizasi
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Il { get; set; }

        [Required]
        [StringLength(100)]
        public string Ilce { get; set; }

        [Required]
        [StringLength(100)]
        public string Mahalle { get; set; }

        [Required]
        [StringLength(50)]
        public string MulkTipi { get; set; }

        [Required]
        [StringLength(20)]
        public string OdaSayisi { get; set; }

        public decimal OrtalamaKira { get; set; }
        public decimal OrtalamaSatisDegeri { get; set; }
        public int AmortismanSuresiYil { get; set; }

        public string YakinDonatilarJSON { get; set; }

        public DateTime SorgulamaTarihi { get; set; }
    }
}
