using System;
using System.Threading.Tasks;

namespace GMK360.Core.Interfaces
{
    public interface IShortTermRentalService
    {
        Task<bool> CheckAvailabilityAsync(int unitId, DateTime startDate, DateTime endDate);
        Task<int?> CreateReservationAsync(int unitId, string tenantUserId, DateTime startDate, DateTime endDate, int guestCount, decimal totalPrice);
        Task<bool> ConfirmReservationAsync(int reservationId);
    }
}
