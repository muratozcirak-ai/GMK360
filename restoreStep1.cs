using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string viewPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string view = File.ReadAllText(viewPath, Encoding.UTF8);

        int start = view.IndexOf("<div class=\"col-md-12\">\r\n                                <label class=\"form-label fw-bold\">Mimari");
        if (start == -1) start = view.IndexOf("<div class=\"col-md-12\">\n                                <label class=\"form-label fw-bold\">Mimari");

        int end = view.IndexOf("</div>\r\n                        </div>\r\n                        <div class=\"text-end mt-5\">", start);
        if (end == -1) end = view.IndexOf("</div>\n                        </div>\n                        <div class=\"text-end mt-5\">", start);

        if (start != -1 && end != -1)
        {
            string newContent = @"<div class=""col-md-12"">
                                <div class=""row g-3"">
                                    <div class=""col-md-6"">
                                        <label class=""form-label fw-bold"">Toplam Arazi Alanı (m²)</label>
                                        <div class=""input-group"">
                                            <input type=""number"" asp-for=""TotalLandArea"" class=""form-control rounded-start-3"" placeholder=""Örn: 2500"" min=""1"" required />
                                            <span class=""input-group-text rounded-end-3"">m²</span>
                                        </div>
                                    </div>
                                    <div class=""col-md-6"">
                                    </div>
                                    <div class=""col-md-6"">
                                        <label class=""form-label fw-bold"">Mevcut Durum Görseli (İlk Hali)</label>
                                        <input asp-for=""CurrentStateImageFile"" type=""file"" class=""form-control rounded-3"" accept=""image/jpeg,image/png,application/pdf"" />
                                        <div class=""form-text text-muted"">Şantiyenin/arazinin şu anki hali (JPG/PNG).</div>
                                    </div>
                                    <div class=""col-md-6"">
                                        <label class=""form-label fw-bold"">Proje Görseli (Geleceği Hali)</label>
                                        <input asp-for=""CoverImageFile"" type=""file"" class=""form-control rounded-3"" accept=""image/jpeg,image/png,application/pdf"" />
                                        <div class=""form-text text-muted"">Mimari 3D render görselini seçin (JPG/PNG).</div>
                                    </div>
                                </div>
                            </div>";
                            
            view = view.Substring(0, start) + newContent + view.Substring(end);
            File.WriteAllText(viewPath, view, new UTF8Encoding(true));
            Console.WriteLine("Restored Step 1 UI.");
        }
        else
        {
            Console.WriteLine($"start: {start}, end: {end}");
        }
    }
}
