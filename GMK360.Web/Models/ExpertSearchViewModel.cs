namespace GMK360.Web.Models
{
    public class ExpertSearchViewModel
    {
        public string Id { get; set; } // Can be string (User Id) or int (ServiceProvider Id)
        public string Type { get; set; } // "Emlak Danışmanı", "Firma", "Usta"
        public string Name { get; set; } // FullName or BusinessName
        public string AvatarUrl { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsVerified { get; set; }
        public string Location { get; set; } // e.g. "Kadıköy, İstanbul"
        public string[] Categories { get; set; } // Expertise areas
        public string Phone { get; set; }
        
        // Navigation Links
        public string ProfileUrl { get; set; }
    }
}
