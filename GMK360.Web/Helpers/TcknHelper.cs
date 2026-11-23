using System.Linq;

namespace GMK360.Web.Helpers
{
    /// <summary>
    /// Resmi 11 haneli T.C. Kimlik Numarası doğrulama ve normalizasyon yardımcısı.
    /// Aynı kişinin farklı e-posta adresleriyle birden fazla "1 Ücretsiz Daire" hakkı açmasını engellemek için kullanılır.
    /// </summary>
    public static class TcknHelper
    {
        public static string Normalize(string? rawTc)
        {
            if (string.IsNullOrWhiteSpace(rawTc)) return string.Empty;
            return new string(rawTc.Where(char.IsDigit).ToArray());
        }

        public static bool IsValidTcIdentity(string? rawTc)
        {
            var tc = Normalize(rawTc);
            if (tc.Length != 11) return false;
            if (tc[0] == '0') return false;

            int[] d = tc.Select(c => c - '0').ToArray();
            int oddSum = d[0] + d[2] + d[4] + d[6] + d[8];
            int evenSum = d[1] + d[3] + d[5] + d[7];

            int check10 = ((oddSum * 7) - evenSum) % 10;
            if (check10 < 0) check10 += 10;
            if (check10 != d[9]) return false;

            int first10Sum = 0;
            for (int i = 0; i < 10; i++) first10Sum += d[i];
            int check11 = first10Sum % 10;
            return check11 == d[10];
        }
    }
}
