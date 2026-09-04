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

        if (!vm.Contains("public double? BaseArea { get; set; }"))
        {
            vm = vm.Replace("public string BlockName { get; set; }", "public string BlockName { get; set; }\n        public double? BaseArea { get; set; }");
            File.WriteAllText(vmPath, vm, new UTF8Encoding(true));
        }

        // 2. Update Controller
        string ctrlPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string ctrl = File.ReadAllText(ctrlPath, Encoding.UTF8);
        if (!ctrl.Contains("BaseArea = b.BaseArea"))
        {
            ctrl = ctrl.Replace("HasRoof = b.HasRoof;", "HasRoof = b.HasRoof,\n                              BaseArea = b.BaseArea;");
            File.WriteAllText(ctrlPath, ctrl, new UTF8Encoding(true));
        }

        // 3. Update View Create.cshtml (Step 2 Blocks template)
        string viewPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string view = File.ReadAllText(viewPath, Encoding.UTF8);

        // Current JS template:
        // <div class=""col-12 col-lg-3"">
        //     <label class=""form-label small fw-bold"">Blok Adı</label>
        //     <input type=""text"" name=""Blocks[${i}].BlockName"" class=""form-control b-name"" placeholder=""A Blok"" required />
        // </div>
        // Let's add BaseArea after BlockName. Wait, what about column widths? 
        // Currently it's:
        // BlockName: 3
        // TotalFloors: 2
        // BasementFloors: 2
        // TotalApartments: 2
        // TotalShops: 2
        // HasRoof: 2 (Wait, my previous script made it col-12 col-lg-2)
        // Total is 3+2+2+2+2+2 = 13 which overflows 12.
        
        string oldNameHtml = @"<div class=""col-12 col-lg-3"">
                                      <label class=""form-label small fw-bold"">Blok Adı</label>
                                      <input type=""text"" name=""Blocks[${i}].BlockName"" class=""form-control b-name"" placeholder=""A Blok"" required />
                                  </div>";
                                  
        string newNameHtml = @"<div class=""col-12 col-lg-2"">
                                      <label class=""form-label small fw-bold"">Blok Adı</label>
                                      <input type=""text"" name=""Blocks[${i}].BlockName"" class=""form-control b-name"" placeholder=""A Blok"" required />
                                  </div>
                                  <div class=""col-12 col-lg-2"">
                                      <label class=""form-label small fw-bold"">Taban (m²)</label>
                                      <input type=""number"" name=""Blocks[${i}].BaseArea"" class=""form-control"" placeholder=""Örn: 200"" min=""1"" />
                                  </div>";

        if (view.Contains(oldNameHtml))
        {
            view = view.Replace(oldNameHtml, newNameHtml);
            File.WriteAllText(viewPath, view, new UTF8Encoding(true));
            Console.WriteLine("Added BaseArea to Wizard.");
        }
        else
        {
            Console.WriteLine("Regex for BaseArea failed in view.");
        }
    }
}
