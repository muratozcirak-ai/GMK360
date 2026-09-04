using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        // 1. Add to Building.cs
        string entityPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\Building.cs";
        string entity = File.ReadAllText(entityPath, Encoding.UTF8);
        if (!entity.Contains("public bool HasRoof"))
        {
            string oldProp = @"public bool HasGroundFloor { get; set; }";
            string newProp = @"public bool HasGroundFloor { get; set; }
        public bool HasRoof { get; set; }";
            entity = entity.Replace(oldProp, newProp);
            File.WriteAllText(entityPath, entity, new UTF8Encoding(true));
        }

        // 2. Add to CreateProjectWizardViewModel.cs
        string vmPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Models\CreateProjectWizardViewModel.cs";
        string vm = File.ReadAllText(vmPath, Encoding.UTF8);
        if (!vm.Contains("public bool HasRoof"))
        {
            string oldProp2 = @"public int TotalShops { get; set; }";
            string newProp2 = @"public int TotalShops { get; set; }
        public bool HasRoof { get; set; }";
            vm = vm.Replace(oldProp2, newProp2);
            File.WriteAllText(vmPath, vm, new UTF8Encoding(true));
        }

        // 3. Add to Create.cshtml
        string viewPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string view = File.ReadAllText(viewPath, Encoding.UTF8);
        string newHtml = @"<div class=""col-6 col-lg-2"">
                                      <label class=""form-label small fw-bold"">Bodrum</label>
                                      <input type=""number"" name=""Blocks[${i}].BasementFloors"" class=""form-control b-basements"" value=""1"" min=""0"" required />
                                  </div>
                                  <div class=""col-12 col-lg-2 d-flex align-items-end"">
                                      <div class=""form-check mb-2"">
                                          <input class=""form-check-input"" type=""checkbox"" name=""Blocks[${i}].HasRoof"" value=""true"" id=""roof_${i}"">
                                          <label class=""form-check-label small fw-bold"" for=""roof_${i}"">Çatı Katı</label>
                                      </div>
                                  </div>";
        string oldHtml = @"<div class=""col-6 col-lg-2"">
                                      <label class=""form-label small fw-bold"">Bodrum</label>
                                      <input type=""number"" name=""Blocks[${i}].BasementFloors"" class=""form-control b-basements"" value=""1"" min=""0"" required />
                                  </div>";
        // Carefully replace the exact old html block
        if(Regex.IsMatch(view, @"<div class=""col-6 col-lg-2"">\s*<label class=""form-label small fw-bold"">Bodrum</label>\s*<input type=""number"" name=""Blocks\[\$\{i\}\]\.BasementFloors"".*?</div>", RegexOptions.Singleline))
        {
            view = Regex.Replace(view, @"<div class=""col-6 col-lg-2"">\s*<label class=""form-label small fw-bold"">Bodrum</label>\s*<input type=""number"" name=""Blocks\[\$\{i\}\]\.BasementFloors"".*?</div>", newHtml, RegexOptions.Singleline);
            
            // Also need to adjust col sizes of others so it fits in a row. 3+2+2+2+2 = 11, it fits.
            // Wait, what were the col sizes?
            // BlockName: col-12 col-lg-3
            // TotalFloors: col-6 col-lg-2
            // BasementFloors: col-6 col-lg-2
            // TotalApartments: col-6 col-lg-2
            // TotalShops: col-6 col-lg-2
            // Total is 11. Adding another col-lg-1 or 2 is fine, it will wrap to next row on lg.
            // Let's just do col-6 col-lg-12 if it doesn't fit, bootstrap flex wraps gracefully.
            File.WriteAllText(viewPath, view, new UTF8Encoding(true));
        }

        // 4. Update Controller (Remove Auto-Generation & map HasRoof)
        string ctrlPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string ctrl = File.ReadAllText(ctrlPath, Encoding.UTF8);

        // Remove the block of unit auto generation
        string pattern = @"// 1\. Bodrum Katlar.*?TempData\[""SuccessMessage""\] = \$\""{project\.Name} projesi ve bloklar başarıyla oluşturuldu\."";";
        string replacement = @"
                          building.HasRoof = b.HasRoof;
                          
                          // Biz otomatik daire oluşturmayı bıraktık! 
                          // Katlar ve daireler ManageBlock ekranından manuel + kopyalama ile yapılacak.
                      }
                  }
                  
                  TempData[""SuccessMessage""] = $""{project.Name} projesi ve bloklar başarıyla oluşturuldu."";
        ";
        
        if (Regex.IsMatch(ctrl, pattern, RegexOptions.Singleline))
        {
            ctrl = Regex.Replace(ctrl, pattern, replacement, RegexOptions.Singleline);
            File.WriteAllText(ctrlPath, ctrl, new UTF8Encoding(true));
        }
        else
        {
            Console.WriteLine("Regex for removing auto-gen failed.");
        }
    }
}
