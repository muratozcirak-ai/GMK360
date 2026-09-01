using System.Threading.Tasks;
using GMK360.Core.Entities;

namespace GMK360.Core.Services
{
    public interface IKbsServisi
    {
        Task<bool> MisafirGirisBildirAsync(int guestCheckInRecordId);
        Task<bool> MisafirCikisBildirAsync(int guestCheckInRecordId);
    }
}
