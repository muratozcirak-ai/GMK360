using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string view = File.ReadAllText(path, Encoding.UTF8);

        // 1) Fix safeCount to count
        view = view.Replace("safeCount === 1", "count === 1");

        // 2) Insert Address Fields before StartDate
        string startDateLabel = "<div class=\"col-md-6\">\r\n                                <label class=\"form-label fw-bold\">Planlanan";
        if (!view.Contains(startDateLabel)) {
            startDateLabel = "<div class=\"col-md-6\">\n                                <label class=\"form-label fw-bold\">Planlanan";
        }
        
        string addressHtml = @"
                            <div class=""col-md-12"">
                                <label class=""form-label fw-bold"">Açık Adres (Tam lokasyon)</label>
                                <textarea asp-for=""Address"" id=""fullAddress"" class=""form-control rounded-3"" rows=""2"" placeholder=""Örn: Cumhuriyet Mah. Vatan Cad. No:16/A"" required></textarea>
                            </div>
                            <div class=""col-md-3"">
                                <label class=""form-label fw-bold"">İl</label>
                                <select asp-for=""CityId"" class=""form-select rounded-3"" asp-items=""ViewBag.Cities"" required><option value="""">Seçiniz</option></select>
                            </div>
                            <div class=""col-md-3"">
                                <label class=""form-label fw-bold"">İlçe</label>
                                <select asp-for=""DistrictId"" class=""form-select rounded-3"" asp-items=""ViewBag.Districts"" required><option value="""">Seçiniz</option></select>
                            </div>
                            <div class=""col-md-3"">
                                <label class=""form-label fw-bold"">Mahalle</label>
                                <select asp-for=""NeighborhoodId"" class=""form-select rounded-3"" asp-items=""ViewBag.Neighborhoods"" required><option value="""">Seçiniz</option></select>
                            </div>
                            <div class=""col-md-3"">
                                <label class=""form-label fw-bold"">Sokak</label>
                                <select asp-for=""StreetId"" class=""form-select rounded-3"" asp-items=""ViewBag.Streets"" required><option value="""">Seçiniz</option></select>
                            </div>
                            ";
                            
        if (!view.Contains("id=\"fullAddress\""))
        {
            view = view.Replace(startDateLabel, addressHtml + startDateLabel);
        }

        // 3) Insert JS Address formatter
        string jsFormatter = @"
        // Adres Formatlayıcı
        document.getElementById('fullAddress').addEventListener('input', function(e) {
            let val = e.target.value;
            // 1. Türkçe karakter ve noktalama kalsın, garip sembolleri uçur.
            val = val.replace(/[^a-zA-Z0-9çÇğĞıİöÖşŞüÜ\s\.,:\/-]/g, '');
            
            // 2. 16a veya 16A yazılırsa 16/A'ya çevir
            val = val.replace(/(\d+)([a-zA-ZçÇğĞıİöÖşŞüÜ])/g, '$1/$2');
            
            // 3. / yanındaki harfi BÜYÜT
            val = val.replace(/\/([a-zçğıöşü])/g, function(match, p1) {
                return '/' + p1.toLocaleUpperCase('tr-TR');
            });
            
            if(e.target.value !== val) {
                const start = e.target.selectionStart;
                const end = e.target.selectionEnd;
                e.target.value = val;
                e.target.setSelectionRange(start, end);
            }
        });
        
        function toggleParent(sel)";
        
        if (!view.Contains("fullAddress'.addEventListener"))
        {
            view = view.Replace("function toggleParent(sel)", jsFormatter);
        }

        File.WriteAllText(path, view, new UTF8Encoding(true));
        Console.WriteLine("All missing features restored and bugs fixed.");
    }
}
