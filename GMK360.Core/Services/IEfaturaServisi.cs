using System.Threading.Tasks;
using GMK360.Core.DTOs;
using GMK360.Core.Entities;

namespace GMK360.Core.Services
{
    public interface IEfaturaServisi
    {
        Task<KesilenFatura> FaturaKesAsync(FaturaGonderimDTO faturadto);
        Task<FaturaDurumu> FaturaDurumSorgulaAsync(string ettn);
    }
}
