using System;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Core.Interfaces;

namespace GMK360.Core.Services
{
    public class TradesmanService : ITradesmanService
    {
        private readonly IRepository<RenovationRequest> _renovationRepository;
        private readonly IRepository<UniversalSurvey> _surveyRepository;

        public TradesmanService(
            IRepository<RenovationRequest> renovationRepository,
            IRepository<UniversalSurvey> surveyRepository)
        {
            _renovationRepository = renovationRepository;
            _surveyRepository = surveyRepository;
        }

        public async Task<bool> AssignJobToTradesmanAsync(int renovationRequestId, string tradesmanUserId)
        {
            var request = await _renovationRepository.GetByIdAsync(renovationRequestId);
            if (request == null) return false;

            request.AssigneeUserId = tradesmanUserId;
            request.Status = RenovationStatus.Assigned;
            request.UpdatedAt = DateTime.UtcNow;

            await _renovationRepository.UpdateAsync(request);
            return true;
        }

        public async Task<bool> AssignJobToSupplierAsync(int renovationRequestId, string supplierUserId)
        {
            var request = await _renovationRepository.GetByIdAsync(renovationRequestId);
            if (request == null) return false;

            request.AssignedSupplierId = supplierUserId;
            request.Status = RenovationStatus.Assigned;
            request.UpdatedAt = DateTime.UtcNow;

            await _renovationRepository.UpdateAsync(request);
            return true;
        }

        public async Task<bool> CompleteJobAndTriggerSurveyAsync(int renovationRequestId, int rating, string comments)
        {
            var request = await _renovationRepository.GetByIdAsync(renovationRequestId);
            if (request == null || (request.AssigneeUserId == null && request.AssignedSupplierId == null)) 
                return false;

            // İş tamamlandı
            request.Status = RenovationStatus.Completed;
            request.UpdatedAt = DateTime.UtcNow;
            await _renovationRepository.UpdateAsync(request);

            // Anket/Değerlendirme oluştur
            var targetUserId = request.AssigneeUserId ?? request.AssignedSupplierId;
            
            var survey = new UniversalSurvey
            {
                RenovationRequestId = renovationRequestId,
                SenderUserId = request.UserId, // İşi veren kişi (Müşteri/Yönetici)
                TargetUserId = targetUserId, // İşi yapan kişi (Usta/Esnaf)
                Rating = rating,
                Comments = comments,
                IsApprovedForPublic = rating >= 4 // 4 ve üzeri puanlar otomatik onaylı referans olsun örneğin
            };

            await _surveyRepository.AddAsync(survey);
            return true;
        }
    }
}
