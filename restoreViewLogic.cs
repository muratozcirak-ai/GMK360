using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string code = File.ReadAllText(path, Encoding.UTF8);

        // We need to inject the HTML back into the template literal inside `generateBlocks()`
        // Let's find: <input type="number" name="Blocks[${i}].TotalFloors" class="form-control" placeholder="Örn: 5" min="0" required />
        //                                       </div>
        
        string find = @"<input type=""number"" name=""Blocks\[\$\{i\}\]\.TotalFloors"" class=""form-control"" placeholder="".*?"" min=""0"" required \/>\s*<\/div>";
        
        string replace = @"<input type=""number"" name=""Blocks[${i}].TotalFloors"" class=""form-control"" placeholder=""Örn: 5"" min=""0"" required />
                                      </div>
                                      <div class=""col-12 col-lg-2"">
                                          <label class=""form-label small fw-bold"">Daire Sayısı</label>
                                          <input type=""number"" name=""Blocks[${i}].TotalApartments"" class=""form-control"" placeholder=""Örn: 20"" min=""0"" required />
                                      </div>
                                      <div class=""col-12 col-lg-2"">
                                          <label class=""form-label small fw-bold"">Dükkan Sayısı</label>
                                          <input type=""number"" name=""Blocks[${i}].TotalShops"" class=""form-control"" placeholder=""Örn: 2"" min=""0"" required />
                                      </div>";

        if (!code.Contains("Blocks[${i}].TotalApartments"))
        {
            code = Regex.Replace(code, find, replace);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Restored Block UI fields in Javascript");
        }
    }
}
