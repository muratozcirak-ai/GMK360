using System.Threading.Tasks;

namespace GMK360.Core.Interfaces
{
    public interface IBuildingManagerService
    {
        Task CalculateAndDistributeExpenseAsync(int expenseId);
        Task AddTenantToUnitAsync(int unitId, string tenantName, string tenantEmail, string tenantPhone);
    }
}
