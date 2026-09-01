using System;
using System.Linq;

namespace GMK360.Core.Utilities
{
    public static class ValidationHelper
    {
        public static bool IsTcknValid(string tckn)
        {
            if (string.IsNullOrWhiteSpace(tckn) || tckn.Length != 11 || !tckn.All(char.IsDigit))
                return false;

            if (tckn[0] == '0')
                return false;

            int[] digits = tckn.Select(c => int.Parse(c.ToString())).ToArray();

            int sumOdd = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
            int sumEven = digits[1] + digits[3] + digits[5] + digits[7];

            int tenthDigit = ((sumOdd * 7) - sumEven) % 10;
            if (tenthDigit < 0) tenthDigit += 10;
            
            if (digits[9] != tenthDigit)
                return false;

            int totalSum = digits.Take(10).Sum();
            if (totalSum % 10 != digits[10])
                return false;

            return true;
        }

        public static bool IsVknValid(string vkn)
        {
            if (string.IsNullOrWhiteSpace(vkn) || vkn.Length != 10 || !vkn.All(char.IsDigit))
                return false;

            int[] digits = vkn.Select(c => int.Parse(c.ToString())).ToArray();
            int sum = 0;

            for (int i = 0; i < 9; i++)
            {
                int tmp = (digits[i] + (10 - (i + 1))) % 10;
                if (tmp == 9)
                {
                    sum += tmp;
                }
                else
                {
                    int prm = (int)(tmp * Math.Pow(2, 9 - (i + 1))) % 9;
                    if (tmp != 0 && prm == 0) prm = 9;
                    sum += prm;
                }
            }

            int lastDigit = (10 - (sum % 10)) % 10;

            return digits[9] == lastDigit;
        }
    }
}
