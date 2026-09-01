using System;

namespace GMK360.Core.Entities
{
    public class PropertyViewHistory : BaseEntity
    {
        public int PropertyId { get; set; }
        public Property Property { get; set; }

        public DateTime ViewDate { get; set; }
        
        // Tekil ziyaretçi sayısını tutmak veya güvenlik (bot) engellemesi için IP Adresi
        public string IpAddress { get; set; }
    }
}
