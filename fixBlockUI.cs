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

        string find = @"<input type=""number"" name=""Blocks\[\$\{i\}\]\.TotalFloors"" class=""form-control b-floors"" value=""5"" min=""0"" required />\s*</div>";
        
        string replace = @"<input type=""number"" name=""Blocks[${i}].TotalFloors"" class=""form-control b-floors"" value=""5"" min=""0"" required />
                                    </div>
                                    <div class=""col-6 col-lg-1"">
                                        <label class=""form-label small fw-bold"">Daire</label>
                                        <input type=""number"" name=""Blocks[${i}].TotalApartments"" class=""form-control"" value=""20"" min=""0"" required />
                                    </div>
                                    <div class=""col-6 col-lg-1"">
                                        <label class=""form-label small fw-bold"">Dükkan</label>
                                        <input type=""number"" name=""Blocks[${i}].TotalShops"" class=""form-control"" value=""2"" min=""0"" required />
                                    </div>";

        if (!code.Contains("Blocks[${i}].TotalApartments"))
        {
            code = Regex.Replace(code, find, replace);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Fixed Block UI fields in Javascript");
        }
        
        // Also inject C# rendering block for existing blocks
        string blocksContainerFind = @"<div id=""blocksContainer"">\s*<!-- Blocks will be injected here via JS -->\s*</div>";
        string csharpRender = @"<div id=""blocksContainer"">
                            @if (Model.Blocks != null && Model.Blocks.Any())
                            {
                                int i = 0;
                                foreach (var b in Model.Blocks)
                                {
                                    <div class=""card border border-2 border-light shadow-sm rounded-4 mb-3 block-item"">
                                        <div class=""card-body p-4"">
                                            <h5 class=""fw-bold text-navy border-bottom pb-2 mb-3""><i class=""ph ph-building me-2 text-orange""></i> @(i+1). Blok / Yapı Tanımı</h5>
                                            <div class=""row g-3"">
                                                <div class=""col-md-12 col-lg-3"">
                                                    <label class=""form-label small fw-bold"">Blok / Yapı Adı</label>
                                                    <input type=""text"" name=""Blocks[@i].BlockName"" class=""form-control b-name"" value=""@b.BlockName"" onkeyup=""updatePodiumDropdowns()"" required  />
                                                    <input type=""hidden"" name=""Blocks[@i].StructureType"" value=""independent"" />
                                                    <input type=""hidden"" name=""Blocks[@i].ParentIndex"" value="""" />
                                                </div>
                                                <div class=""col-12 col-lg-2"">
                                                    <label class=""form-label small fw-bold"">Taban (m²)</label>
                                                    <input type=""number"" name=""Blocks[@i].BaseArea"" class=""form-control"" value=""@b.BaseArea"" min=""1"" required />
                                                </div>
                                                <div class=""col-6 col-lg-2"">
                                                    <label class=""form-label small fw-bold"">Normal Kat Sayısı</label>
                                                    <input type=""number"" name=""Blocks[@i].TotalFloors"" class=""form-control b-floors"" value=""@b.TotalFloors"" min=""0"" required />
                                                </div>
                                                <div class=""col-6 col-lg-1"">
                                                    <label class=""form-label small fw-bold"">Daire</label>
                                                    <input type=""number"" name=""Blocks[@i].TotalApartments"" class=""form-control"" value=""@b.TotalApartments"" min=""0"" required />
                                                </div>
                                                <div class=""col-6 col-lg-1"">
                                                    <label class=""form-label small fw-bold"">Dükkan</label>
                                                    <input type=""number"" name=""Blocks[@i].TotalShops"" class=""form-control"" value=""@b.TotalShops"" min=""0"" required />
                                                </div>
                                                <div class=""col-6 col-lg-2"">
                                                    <label class=""form-label small fw-bold"">Bodrum Kat Sayısı</label>
                                                    <input type=""number"" name=""Blocks[@i].BasementFloors"" class=""form-control b-basements"" value=""@b.BasementFloors"" min=""0"" required />
                                                    <div class=""form-check mt-2"">
                                                        <input class=""form-check-input"" type=""checkbox"" name=""Blocks[@i].HasGroundFloor"" value=""true"" id=""ground_@i"" @(b.HasGroundFloor ? ""checked"" : """")>
                                                        <label class=""form-check-label small fw-bold"" for=""ground_@i"">Zemin Kat Var</label>
                                                    </div>
                                                    <div class=""form-check"">
                                                        <input class=""form-check-input"" type=""checkbox"" name=""Blocks[@i].HasRoof"" value=""true"" id=""roof_@i"" @(b.HasRoof ? ""checked"" : """")>
                                                        <label class=""form-check-label small fw-bold"" for=""roof_@i"">Çatı Katı Var</label>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    i++;
                                }
                            }
                          </div>";
                          
        if (!code.Contains("@foreach (var b in Model.Blocks)"))
        {
            code = Regex.Replace(code, blocksContainerFind, csharpRender);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Fixed Server Side Block Rendering");
        }
    }
}
