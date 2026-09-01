using System;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Core.Interfaces;

namespace GMK360.Web.Services
{
    public class ShortTermRentalService : IShortTermRentalService
    {
        private readonly GMK360.Data.Contexts.ApplicationDbContext _context;

        public ShortTermRentalService(GMK360.Data.Contexts.ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CheckAvailabilityAsync(int unitId, DateTime startDate, DateTime endDate)
        {
            var unit = await _context.BuildingUnits.FindAsync(unitId);
            if (unit == null || !unit.IsAvailableForDailyRent) return false;

            // Kesişen rezervasyon var mı?
            var overlappingReservations = _context.Reservations.Where(r => 
                r.UnitId == unitId && 
                r.ReservationStatus != ReservationStatus.Cancelled &&
                ((startDate >= r.StartDate && startDate < r.EndDate) ||
                 (endDate > r.StartDate && endDate <= r.EndDate) ||
                 (startDate <= r.StartDate && endDate >= r.EndDate))
            );

            return !overlappingReservations.Any();
        }

        public async Task<int?> CreateReservationAsync(int unitId, string tenantUserId, DateTime startDate, DateTime endDate, int guestCount, decimal totalPrice)
        {
            var isAvailable = await CheckAvailabilityAsync(unitId, startDate, endDate);
            if (!isAvailable) return null;

            var reservation = new Reservation
            {
                UnitId = unitId,
                TenantUserId = tenantUserId,
                StartDate = startDate,
                EndDate = endDate,
                GuestCount = guestCount,
                TotalPrice = totalPrice,
                ReservationStatus = ReservationStatus.Pending
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            return reservation.Id;
        }

        public async Task<bool> ConfirmReservationAsync(int reservationId)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation == null) return false;

            reservation.ReservationStatus = ReservationStatus.Confirmed;
            reservation.UpdatedAt = DateTime.UtcNow;

            _context.Reservations.Update(reservation);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
