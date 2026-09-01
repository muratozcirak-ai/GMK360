using System.Threading.Tasks;
using GMK360.Core.Entities;

namespace GMK360.Web.Services
{
    public interface IAppointmentService
    {
        Task<(bool IsSuccess, string Message)> RequestAppointmentAsync(ServiceAppointment appointment);
        Task<(bool IsSuccess, string Message)> ConfirmAppointmentAsync(int appointmentId, int providerId);
    }
}
