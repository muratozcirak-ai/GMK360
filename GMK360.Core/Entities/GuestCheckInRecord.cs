using System;

namespace GMK360.Core.Entities
{
    public class GuestCheckInRecord : BaseEntity
    {
        public int ReservationId { get; set; }
        public PropertyReservation Reservation { get; set; }

        public string TcOrPassportNo { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Nationality { get; set; }
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; } // Erkek, Kadın vb. KBS için zorunlu
        
        public bool IsPrimaryGuest { get; set; } // Asıl kiralayan kişi (Sözleşme muhatabı)
        
        public string Status { get; set; } // Pending (Misafir doldurdu), CheckedIn (Emniyete gitti)
        public KbsBildirimDurumu KbsStatus { get; set; } = KbsBildirimDurumu.Bekliyor;
    }

    public enum KbsBildirimDurumu
    {
        Bekliyor,
        GirisBildirildi,
        CikisBildirildi,
        Hata
    }
}
