using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        // 1. Update View Model
        string vmPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Models\CreateProjectWizardViewModel.cs";
        string vm = File.ReadAllText(vmPath, Encoding.UTF8);

        if (!vm.Contains("public double? TotalLandArea"))
        {
            vm = vm.Replace("public double? Longitude;", "public double? Longitude;\n\n        public double? TotalLandArea { get; set; }");
            File.WriteAllText(vmPath, vm, new UTF8Encoding(true));
        }

        // 2. Update Controller
        string ctrlPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string ctrl = File.ReadAllText(ctrlPath, Encoding.UTF8);
        if (!ctrl.Contains("TotalLandArea = model.TotalLandArea"))
        {
            ctrl = ctrl.Replace("Status = 1,", "Status = 1,\n                TotalLandArea = model.TotalLandArea,");
            File.WriteAllText(ctrlPath, ctrl, new UTF8Encoding(true));
        }

        // 3. Update View
        string viewPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string view = File.ReadAllText(viewPath, Encoding.UTF8);

        string targetHtml = @"<div class=""col-md-6"">
                                  <label class=""form-label fw-bold"">Mevcut Durum Grseli (lk Hali)</label>";
                                  
        string newHtml = @"<div class=""col-12 mt-4 mb-2"">
                                  <h6 class=""fw-bold text-navy mb-3""><i class=""ph ph-ruler me-2""></i>Proje Alan</h6>
                                  <div class=""row g-3"">
                                      <div class=""col-md-6"">
                                          <label class=""form-label fw-bold"">Proje Toplam Arazi Alan (m)</label>
                                          <div class=""input-group"">
                                              <input type=""number"" asp-for=""TotalLandArea"" class=""form-control rounded-start-3"" placeholder=""rn: 2500"" min=""1"" required />
                                              <span class=""input-group-text rounded-end-3"">m</span>
                                          </div>
                                      </div>
                                  </div>
                              </div>
                              
                              <div class=""col-md-6"">
                                  <label class=""form-label fw-bold"">Mevcut Durum Grseli (lk Hali)</label>";
                                  
        // In the database the text is encoded strangely for Turkish characters in PowerShell context, but in C# it's usually valid UTF-8.
        // I will use exact string matching from the raw file bytes for safety.
        
        // Let's just find "Mevcut Durum G" and insert before it.
        int idx = view.IndexOf(@"<div class=""col-md-6"">
                                  <label class=""form-label fw-bold"">Mevcut Durum");
        
        if (idx != -1 && !view.Contains("TotalLandArea"))
        {
            string cleanHtml = @"<div class=""col-12 mt-4 mb-2"">
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
            view = view.Insert(idx, cleanHtml);
            File.WriteAllText(viewPath, view, new UTF8Encoding(true));
        }

        Console.WriteLine("Added TotalLandArea to Wizard.");
    }
}
