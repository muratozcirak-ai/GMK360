using System.Threading.Tasks;

namespace GMK360.Core.Interfaces
{
    public interface IScheduleGeneratorService
    {
        /// <summary>
        /// Mülk ve Kiracı/EvSahibi rollerine göre yıllık vergi ve finansal takvimi (Schedule) otomatik oluşturur.
        /// </summary>
        /// <param name="propertyId">İşlem yapılacak mülkün ID'si</param>
        /// <param name="taxYear">Hangi yılın takvimi oluşturulacak? (Örn: 2026)</param>
        Task GenerateSchedulesForPropertyAsync(int propertyId, int taxYear);
    }
}
