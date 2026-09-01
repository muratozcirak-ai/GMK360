using System;
using System.Threading.Tasks;
using GMK360.Core.Entities;

namespace GMK360.Core.Interfaces
{
    public interface IInvitationService
    {
        Task<Guid> InviteUserAsync(string inviterUserId, string targetEmailOrPhone, InvitationRole role, int? relatedUnitId = null);
        Task<bool> AcceptInvitationAsync(Guid token, string newUserId);
    }
}
