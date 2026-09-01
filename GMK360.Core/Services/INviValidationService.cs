using System.Threading.Tasks;

namespace GMK360.Core.Services
{
    public interface INviValidationService
    {
        Task<bool> ValidateTcIdentityAsync(string tcIdentityNo, string firstName, string lastName, int birthYear);
    }
}
