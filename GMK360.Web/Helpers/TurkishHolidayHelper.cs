using System;
using System.Collections.Generic;

namespace GMK360.Web.Helpers
{
    public sealed class TurkishHolidayItem
    {
        public DateTime Date { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal DayWeight { get; set; } = 1.0m; // 0.5 = Arife yarım gün
        public string Category { get; set; } = "Resmi";
    }

    /// <summary>
    /// Yıllık resmi ve dini tatil takvimi (Bayram mesaisi çift yevmiye tahakkuk referansı).
    /// </summary>
    public static class TurkishHolidayHelper
    {
        public static List<TurkishHolidayItem> GetHolidaysForYear(int year)
        {
            var list = new List<TurkishHolidayItem>
            {
                new() { Date = new DateTime(year, 1, 1), Name = "Yılbaşı", Category = "Resmi" },
                new() { Date = new DateTime(year, 4, 23), Name = "23 Nisan Ulusal Egemenlik ve Çocuk Bayramı", Category = "Resmi" },
                new() { Date = new DateTime(year, 5, 1), Name = "1 Mayıs Emek ve Dayanışma Günü", Category = "Resmi" },
                new() { Date = new DateTime(year, 5, 19), Name = "19 Mayıs Atatürk'ü Anma, Gençlik ve Spor Bayramı", Category = "Resmi" },
                new() { Date = new DateTime(year, 7, 15), Name = "15 Temmuz Demokrasi ve Millî Birlik Günü", Category = "Resmi" },
                new() { Date = new DateTime(year, 8, 30), Name = "30 Ağustos Zafer Bayramı", Category = "Resmi" },
                new() { Date = new DateTime(year, 10, 28), Name = "Cumhuriyet Bayramı Arifesi (Öğleden Sonra)", DayWeight = 0.5m, Category = "Resmi" },
                new() { Date = new DateTime(year, 10, 29), Name = "29 Ekim Cumhuriyet Bayramı", Category = "Resmi" }
            };

            if (year == 2026)
            {
                list.AddRange(new TurkishHolidayItem[]
                {
                    new() { Date = new DateTime(2026, 3, 19), Name = "Ramazan Bayramı Arifesi", DayWeight = 0.5m, Category = "Dini" },
                    new() { Date = new DateTime(2026, 3, 20), Name = "Ramazan Bayramı 1. Gün", Category = "Dini" },
                    new() { Date = new DateTime(2026, 3, 21), Name = "Ramazan Bayramı 2. Gün", Category = "Dini" },
                    new() { Date = new DateTime(2026, 3, 22), Name = "Ramazan Bayramı 3. Gün", Category = "Dini" },
                    new() { Date = new DateTime(2026, 5, 26), Name = "Kurban Bayramı Arifesi", DayWeight = 0.5m, Category = "Dini" },
                    new() { Date = new DateTime(2026, 5, 27), Name = "Kurban Bayramı 1. Gün", Category = "Dini" },
                    new() { Date = new DateTime(2026, 5, 28), Name = "Kurban Bayramı 2. Gün", Category = "Dini" },
                    new() { Date = new DateTime(2026, 5, 29), Name = "Kurban Bayramı 3. Gün", Category = "Dini" },
                    new() { Date = new DateTime(2026, 5, 30), Name = "Kurban Bayramı 4. Gün", Category = "Dini" }
                });
            }
            else if (year == 2027)
            {
                list.AddRange(new TurkishHolidayItem[]
                {
                    new() { Date = new DateTime(2027, 3, 8), Name = "Ramazan Bayramı Arifesi", DayWeight = 0.5m, Category = "Dini" },
                    new() { Date = new DateTime(2027, 3, 9), Name = "Ramazan Bayramı 1. Gün", Category = "Dini" },
                    new() { Date = new DateTime(2027, 3, 10), Name = "Ramazan Bayramı 2. Gün", Category = "Dini" },
                    new() { Date = new DateTime(2027, 3, 11), Name = "Ramazan Bayramı 3. Gün", Category = "Dini" },
                    new() { Date = new DateTime(2027, 5, 15), Name = "Kurban Bayramı Arifesi", DayWeight = 0.5m, Category = "Dini" },
                    new() { Date = new DateTime(2027, 5, 16), Name = "Kurban Bayramı 1. Gün", Category = "Dini" },
                    new() { Date = new DateTime(2027, 5, 17), Name = "Kurban Bayramı 2. Gün", Category = "Dini" },
                    new() { Date = new DateTime(2027, 5, 18), Name = "Kurban Bayramı 3. Gün", Category = "Dini" },
                    new() { Date = new DateTime(2027, 5, 19), Name = "Kurban Bayramı 4. Gün", Category = "Dini" }
                });
            }

            list.Sort((a, b) => a.Date.CompareTo(b.Date));
            return list;
        }
    }
}
