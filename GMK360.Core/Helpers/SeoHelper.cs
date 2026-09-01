using System;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;

namespace GMK360.Core.Helpers
{
    public static class SeoHelper
    {
        public static string ToSeoUrl(this string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            // Türkçe karakterleri çevir
            text = text.ToLower(new CultureInfo("tr-TR"));
            text = text.Replace("ğ", "g")
                       .Replace("ü", "u")
                       .Replace("ş", "s")
                       .Replace("ı", "i")
                       .Replace("ö", "o")
                       .Replace("ç", "c");

            // Geçersiz karakterleri tireye çevir
            text = Regex.Replace(text, @"[^a-z0-9\s-]", "");
            
            // Birden fazla boşluğu tek tireye çevir
            text = Regex.Replace(text, @"\s+", "-").Trim();
            
            // Yanyana gelen tireleri teke düşür
            text = Regex.Replace(text, @"-+", "-");

            return text;
        }
    }
}
