using System.Threading.Tasks;

namespace GMK360.Web.Services
{
    public interface IValuationService
    {
        Task RequestValuationAsync(string il, string ilce, string mahalle, string mulkTipi, string odaSayisi, string email);
    }
}
