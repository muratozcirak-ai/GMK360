using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GMK360.Core.Entities;
using GMK360.Data.Contexts;
using GMK360.Web.Services.Sms;
using Microsoft.Extensions.Logging;

namespace GMK360.Web.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISmsService _smsService;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(ApplicationDbContext context, ISmsService smsService, ILogger<AppointmentService> logger)
        {
            _context = context;
            _smsService = smsService;
            _logger = logger;
        }

        public async Task<(bool IsSuccess, string Message)> RequestAppointmentAsync(ServiceAppointment appointment)
        {
            // Çakışma Kontrolü (Conflict Resolution)
            // Ustanın o gün için onaylanmış bir randevusu var mı? (Basitçe o gün için dolu mu kontrolü)
            var hasConflict = await _context.ServiceAppointments
                .AnyAsync(a => a.ServiceProviderId == appointment.ServiceProviderId 
                            && a.AppointmentDate.Date == appointment.AppointmentDate.Date 
                            && a.Status == AppointmentStatus.Confirmed);

            if (hasConflict)
            {
                return (false, "Ustanın seçtiğiniz tarihte başka bir randevusu bulunmaktadır. Lütfen farklı bir tarih seçin.");
            }

            _context.ServiceAppointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Ustaya SMS gönder
            var provider = await _context.ServiceProviders.Include(sp => sp.User).FirstOrDefaultAsync(sp => sp.Id == appointment.ServiceProviderId);
            var customer = await _context.Users.FindAsync(appointment.CustomerId);
            if (provider != null && provider.User != null && !string.IsNullOrEmpty(provider.User.PhoneNumber))
            {
                string msg = $"Yeni bir hizmet talebiniz var. Müşteri: {customer?.FirstName} {customer?.LastName}, Tarih: {appointment.AppointmentDate.ToString("dd.MM.yyyy")}. Onaylamak için sisteme girin.";
                await _smsService.SmsGonderAsync(provider.User.PhoneNumber, msg);
                _logger.LogInformation($"SMS Sent to {provider.User.PhoneNumber}: {msg}");
            }

            return (true, "Randevu talebiniz ustaya iletildi.");
        }

        public async Task<(bool IsSuccess, string Message)> ConfirmAppointmentAsync(int appointmentId, int providerId)
        {
            var appointment = await _context.ServiceAppointments
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.ServiceProviderId == providerId);

            if (appointment == null) return (false, "Randevu bulunamadı.");
            if (appointment.Status != AppointmentStatus.Pending) return (false, "Bu randevu onaylanamaz durumda.");

            // Çakışma kontrolü (beklerken başka randevu onaylanmış olabilir)
            var hasConflict = await _context.ServiceAppointments
                .AnyAsync(a => a.ServiceProviderId == providerId 
                            && a.AppointmentDate.Date == appointment.AppointmentDate.Date 
                            && a.Status == AppointmentStatus.Confirmed);

            if (hasConflict)
            {
                return (false, "Bu tarihte zaten onaylanmış bir randevunuz var.");
            }

            appointment.Status = AppointmentStatus.Confirmed;
            await _context.SaveChangesAsync();

            // Müşteriye SMS gönder
            if (appointment.Customer != null && !string.IsNullOrEmpty(appointment.Customer.PhoneNumber))
            {
                string msg = $"Randevunuz onaylandı! Usta seçtiğiniz tarihte adresinizde olacak.";
                await _smsService.SmsGonderAsync(appointment.Customer.PhoneNumber, msg);
                _logger.LogInformation($"SMS Sent to {appointment.Customer.PhoneNumber}: {msg}");
            }

            return (true, "Randevu başarıyla onaylandı.");
        }
    }
}
