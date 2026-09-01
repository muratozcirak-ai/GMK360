using System;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Core.Interfaces;

namespace GMK360.Web.Services
{
    public class InvitationService : IInvitationService
    {
        private readonly GMK360.Data.Contexts.ApplicationDbContext _context;

        public InvitationService(GMK360.Data.Contexts.ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> InviteUserAsync(string inviterUserId, string targetEmailOrPhone, InvitationRole role, int? relatedUnitId = null)
        {
            var invitation = new InvitationToken
            {
                Token = Guid.NewGuid(),
                InviterUserId = inviterUserId,
                TargetEmailOrPhone = targetEmailOrPhone,
                TargetRole = role,
                RelatedUnitId = relatedUnitId,
                ExpirationDate = DateTime.UtcNow.AddDays(7)
            };

            _context.InvitationTokens.Add(invitation);
            await _context.SaveChangesAsync();
            // Burada gerçek hayatta SMS veya E-posta gönderme servisi tetiklenir.
            
            return invitation.Token;
        }

        public async Task<bool> AcceptInvitationAsync(Guid token, string newUserId)
        {
            var invitation = _context.InvitationTokens.FirstOrDefault(i => i.Token == token && !i.IsUsed && i.ExpirationDate > DateTime.UtcNow);

            if (invitation == null) return false;

            invitation.IsUsed = true;
            invitation.UpdatedAt = DateTime.UtcNow;

            _context.InvitationTokens.Update(invitation);

            // İlişkili ünite varsa, yeni kullanıcıyı (Gölge hesaptan gerçek hesaba geçen) o üniteye ata.
            if (invitation.RelatedUnitId.HasValue)
            {
                var unit = await _context.BuildingUnits.FindAsync(invitation.RelatedUnitId.Value);
                if (unit != null)
                {
                    if (invitation.TargetRole == InvitationRole.Tenant)
                    {
                        unit.TenantUserId = newUserId;
                    }
                    else if (invitation.TargetRole == InvitationRole.Owner)
                    {
                        unit.OwnerUserId = newUserId;
                    }
                    _context.BuildingUnits.Update(unit);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
