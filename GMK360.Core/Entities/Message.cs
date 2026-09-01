using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class Message : BaseEntity
    {
        public string SenderId { get; set; }
        public ApplicationUser Sender { get; set; }

        public string ReceiverId { get; set; }
        public ApplicationUser Receiver { get; set; }

        // Opsiyonel: Mesajın belirli bir ilanla ilgili olup olmadığı
        public int? PropertyId { get; set; }
        public Property Property { get; set; }

        public string Content { get; set; }
        public bool IsRead { get; set; }
    }
}
