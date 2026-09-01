using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class ConsultantRating : BaseEntity
    {
        public string ConsultantId { get; set; }
        public ApplicationUser Consultant { get; set; }

        public string UserId { get; set; } // Puan veren müşteri
        public ApplicationUser User { get; set; }

        public int CommunicationScore { get; set; } // 1-5
        public int KnowledgeScore { get; set; } // 1-5
        public int RecommendationScore { get; set; } // 1-5

        public string? Comment { get; set; } // Müşteri yorumu
        public bool IsApproved { get; set; } = false; // Spam/küfür kontrolü için onay havuzu
    }
}
