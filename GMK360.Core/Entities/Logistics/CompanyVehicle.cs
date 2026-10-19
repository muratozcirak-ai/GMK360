using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities.Logistics
{
    public class CompanyVehicle : BaseEntity
    {
        public int AgencyId { get; set; }
        public string PlateNumber { get; set; }
        public string? VehicleType { get; set; } // Pikap, Binek, Minibüs, İş Makinası vs
        public string? BrandModel { get; set; }
        public string Status { get; set; } = "Aktif"; // Aktif, Bakımda, Pasif
        
        // Araç KM ve Saat Takibi
        public int CurrentKm { get; set; } = 0; // Güncel Kilometre
        public decimal CurrentWorkingHours { get; set; } = 0; // İş Makinaları için Güncel Çalışma Saati
        
        // Ruhsat Bilgileri
        public string? RegistrationSerialNo { get; set; } // Ruhsat Belge Seri / Sıra No (Örn: AB 123456)
        public string? RegistrationPhotoPath { get; set; } // Ruhsat Fotoğrafı (Ön/Arka)
        
        // Sigorta & Kasko Poliçe Bilgileri
        public string? InsuranceCompany { get; set; } // Sigorta Şirketi (Allianz, Anadolu vb.)
        public string? InsurancePolicyNo { get; set; } // Poliçe No
        public DateTime? InsuranceExpiryDate { get; set; } // Poliçe Bitiş Tarihi
        public string? InsurancePolicyPhotoPath { get; set; } // Poliçe Belgesi / Fotoğrafı
        
        // Muayene / Vize Takibi
        public DateTime? InspectionExpiryDate { get; set; } // Tüvtürk Muayene Bitiş Tarihi
        
        public ICollection<VehicleAssignment> Assignments { get; set; }
        public ICollection<VehicleTask> Tasks { get; set; }
        public ICollection<VehicleExpense> Expenses { get; set; }
    }

    public class VehicleAssignment : BaseEntity
    {
        public int VehicleId { get; set; }
        public CompanyVehicle Vehicle { get; set; }
        
        public int AgencyId { get; set; }
        public string? AssignedToName { get; set; }
        public string? AssignedToUserId { get; set; }
        
        public int? ProjectId { get; set; } // Hangi şantiyede?
        
        public DateTime AssignmentDate { get; set; } = DateTime.UtcNow;
        public DateTime? ReturnDate { get; set; }
        public string? Notes { get; set; }
    }
}