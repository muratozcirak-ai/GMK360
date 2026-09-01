using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GMK360.Web.Models
{
    public class CreateProjectWizardViewModel
    {
        [Required(ErrorMessage = "Proje adı zorunludur.")]
        public string Name { get; set; }

        public string Description { get; set; }
        
        public string Address { get; set; }
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public IFormFile CoverImageFile { get; set; }

        public List<WizardBlockItem> Blocks { get; set; } = new List<WizardBlockItem>();
    }

    public class WizardBlockItem
    {
        public string BlockName { get; set; }
        public int TotalFloors { get; set; }
        public int TotalApartments { get; set; }
        public int TotalShops { get; set; }
    }
}
