using System;
using System.Collections.Generic;
using GMK360.Core.Entities.Identity;
using GMK360.Core.Entities;

namespace GMK360.Web.Models
{
    public class PublicProfileViewModel
    {
        public string ProfileUserId { get; set; }
        public string DisplayName { get; set; }
        public UserType UserType { get; set; }
        public string ProfileImageUrl { get; set; }
        public string Bio { get; set; }
        public DateTime MemberSince { get; set; }

        // Rozetler
        public bool IsVerifiedUser { get; set; } // NVI (Mavi Onay olmasa da gerek kii)
        public bool IsCorporateVerified { get; set; } // Kurumsal Mavi Onay
        public bool IsEmergencyModeActive { get; set; } // 7/24 Usta (Krmz Rozet)

        public int ReliabilityScore { get; set; }
        
        // Emlak Danman ise
        public List<Property> ActiveListings { get; set; } = new List<Property>();
        
        // Usta ise
        public List<ServiceProviderService> ProviderServices { get; set; } = new List<ServiceProviderService>();
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public string BusinessName { get; set; }
    }
}
