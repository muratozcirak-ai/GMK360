using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GMK360.Core.Entities.B2b
{
    public class B2bCategory : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // Hafriyat, Hazır Beton, İzolasyon, İSG vb.

        public virtual ICollection<B2bCompanyCategory> CompanyCategories { get; set; } = new List<B2bCompanyCategory>();
    }
}
