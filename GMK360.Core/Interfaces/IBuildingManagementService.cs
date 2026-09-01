using System.Collections.Generic;
using System.Threading.Tasks;
using GMK360.Core.Entities;

namespace GMK360.Core.Interfaces
{
    public interface IBuildingManagementService
    {
        Task<List<Building>> GetManagerBuildingsAsync(string managerUserId);
        Task<Building> GetBuildingByIdAsync(int id);
        Task<Building> CreateBuildingAsync(Building building);
        Task<BuildingUnit> AddUnitAsync(BuildingUnit unit);
        Task<BuildingExpense> AddExpenseAsync(BuildingExpense expense, List<int> unitIds);
        Task<BuildingAnnouncement> AddAnnouncementAsync(BuildingAnnouncement announcement);
        Task<UnitDebt> MarkDebtAsPaidAsync(int debtId, string paidBy);
        
        // For residents
        Task<List<BuildingUnit>> GetResidentUnitsAsync(string email);
        Task<BuildingUnit> GetResidentUnitByIdAsync(int unitId, string email);
        Task<List<BuildingAnnouncement>> GetBuildingAnnouncementsAsync(int buildingId);
        Task<List<UnitDebt>> GetUnitDebtsAsync(int unitId);
    }
}
