namespace GMK360.Core.Services.Mobilization
{
    public interface ILaborCostCalculatorService
    {
        (decimal NetWage, decimal SgkAndTax, decimal CampAmortization, decimal TotalCost) CalculateDailyCost(
            int masterCount, decimal masterWage, 
            int workerCount, decimal workerWage, 
            decimal campDailyCostPerPerson);
    }

    public class LaborCostCalculatorService : ILaborCostCalculatorService
    {
        // 2026 Varsayýlan oranlarý
        private const decimal SGK_EMPLOYER_SHARE = 0.205m; // SGK Ýþveren Payý %20.5 (Teþvikli)
        private const decimal UNEMPLOYMENT_EMPLOYER_SHARE = 0.02m; // Ýþsizlik %2

        public (decimal NetWage, decimal SgkAndTax, decimal CampAmortization, decimal TotalCost) CalculateDailyCost(
            int masterCount, decimal masterWage, 
            int workerCount, decimal workerWage, 
            decimal campDailyCostPerPerson)
        {
            int totalPeople = masterCount + workerCount;
            
            // 1. Çýplak Yevmiye
            decimal netWage = (masterCount * masterWage) + (workerCount * workerWage);

            // 2. Görünmez SGK ve Vergi Motoru
            // Gerçekte brütten hesaplanýr ancak prototip için net yevmiye üzerinden oranlýyoruz
            decimal sgkAndTax = netWage * (SGK_EMPLOYER_SHARE + UNEMPLOYMENT_EMPLOYER_SHARE);

            // 3. Kamp ve Ýaþe (Lojistik) Amortismaný
            decimal campAmortization = totalPeople * campDailyCostPerPerson;

            // 4. Patronun "Gerçek Maliyet" Tablosu
            decimal totalCost = netWage + sgkAndTax + campAmortization;

            return (netWage, sgkAndTax, campAmortization, totalCost);
        }
    }
}
