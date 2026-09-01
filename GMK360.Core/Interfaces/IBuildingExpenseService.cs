using System.Threading.Tasks;

namespace GMK360.Core.Interfaces
{
    public interface IBuildingExpenseService
    {
        /// <summary>
        /// Bir daireye (Property) kesilen bina ortak gider payının ödenmesi (Kiracı veya Ev Sahibi tarafından) işlemini yapar.
        /// Demirbaş ise ve kiracı ödemişse kiradan mahsup eder.
        /// Demirbaş (CapitalExpenditure) ise ev sahibinin vergi indirim (PropertyExpense) tablosuna yazar.
        /// </summary>
        Task PayExpenseShareAsync(int expenseShareId, string paidByUserId);
    }
}
