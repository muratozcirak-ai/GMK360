using System;

namespace GMK360.Core.Entities.B2B
{
    public enum QuoteInviteStatus
    {
        Pending = 0,    // Davet Gitti, Bekliyor
        Submitted = 1,  // Fiyat Verdi
        Rejected = 2,   // İlgilenmiyorum dedi
        Accepted = 3    // İşi aldı (Müteahhit onayladı)
    }

    // Tedarikçiye giden teklif formu
    public class B2BQuoteInvite : BaseEntity
    {
        public int QuoteRequestId { get; set; }
        public B2BQuoteRequest QuoteRequest { get; set; }

        public int NetworkContactId { get; set; }
        public B2BNetworkContact NetworkContact { get; set; }

        public QuoteInviteStatus Status { get; set; } = QuoteInviteStatus.Pending;

        public decimal? OfferedPrice { get; set; }
        public string OfferNotes { get; set; }

        public DateTime? RespondedAt { get; set; }
    }
}
