using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace GMK360.Core.Extensions
{
    public static class StringExtensions
    {
        public static string ToUrlSlug(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var normalizedString = value.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    // Türkçe karakter dönüşümleri
                    if (c == 'ı') stringBuilder.Append('i');
                    else if (c == 'ğ') stringBuilder.Append('g');
                    else if (c == 'ü') stringBuilder.Append('u');
                    else if (c == 'ş') stringBuilder.Append('s');
                    else if (c == 'ö') stringBuilder.Append('o');
                    else if (c == 'ç') stringBuilder.Append('c');
                    else if (c == 'İ') stringBuilder.Append('i');
                    else if (c == 'Ğ') stringBuilder.Append('g');
                    else if (c == 'Ü') stringBuilder.Append('u');
                    else if (c == 'Ş') stringBuilder.Append('s');
                    else if (c == 'Ö') stringBuilder.Append('o');
                    else if (c == 'Ç') stringBuilder.Append('c');
                    else stringBuilder.Append(c);
                }
            }

            var str = stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
            
            // Geçersiz karakterleri çıkar
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            
            // Birden fazla boşluğu veya tireyi tek tire yap
            str = Regex.Replace(str, @"[\s-]+", " ").Trim();
            
            // Boşlukları tire yap
            str = str.Replace(" ", "-");

            return str;
        }
    }
}
