using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string viewPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string view = File.ReadAllText(viewPath, Encoding.UTF8);

        if (!view.Contains("TotalLandArea"))
        {
            string cleanHtml = @"
                              <div class=""col-12 mt-4 mb-2"">
                                  <h6 class=""fw-bold text-navy mb-3""><i class=""bi bi-rulers me-2""></i>Proje Alanı</h6>
                                  <div class=""row g-3"">
                                      <div class=""col-md-6"">
                                          <label class=""form-label fw-bold"">Toplam Arazi Alanı (m²)</label>
                                          <div class=""input-group"">
                                              <input type=""number"" asp-for=""TotalLandArea"" class=""form-control rounded-start-3"" placeholder=""Örn: 2500"" min=""1"" required />
                                              <span class=""input-group-text rounded-end-3"">m²</span>
                                          </div>
                                      </div>
                                  </div>
                              </div>
";

            // Find "Mevcut Durum G"
            int idx = view.IndexOf("Mevcut Durum G");
            if (idx != -1)
            {
                // Go backwards to find <div class="col-md-6">
                int divIdx = view.LastIndexOf("<div", idx);
                if (divIdx != -1)
                {
                    view = view.Insert(divIdx, cleanHtml);
                    File.WriteAllText(viewPath, view, new UTF8Encoding(true));
                    Console.WriteLine("Added TotalLandArea.");
                }
            }
        }
    }
}
