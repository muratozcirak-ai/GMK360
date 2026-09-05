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

        string oldBlocksContainer = @"<div id=""blocksContainer"">
                            <!-- Blocks will be injected here via JS -->
                        </div>";

        string newBlocksContainer = @"<div id=""blocksContainer"">
                            @if (Model.Blocks != null && Model.Blocks.Any())
                            {
                                int i = 0;
                                foreach (var b in Model.Blocks)
                                {
                                    <div class=""card border border-2 border-light shadow-sm rounded-4 mb-3 block-item"">
                                        <div class=""card-body p-4"">
                                            <h5 class=""fw-bold text-navy border-bottom pb-2 mb-3""><i class=""ph ph-building me-2 text-orange""></i> @(i + 1). Blok / Yapı Tanımı</h5>
                                            <div class=""row g-3"">
                                                <div class=""col-md-12 col-lg-3"">
                                                    <label class=""form-label small fw-bold"">Blok / Yapı Adı</label>
                                                    <input type=""text"" name=""Blocks[@i].BlockName"" class=""form-control b-name"" value=""@b.BlockName"" required />
                                                </div>
                                                <div class=""col-6 col-lg-2"">
                                                    <label class=""form-label small"">Taban Alanı (m²)</label>
                                                    <input type=""number"" name=""Blocks[@i].BaseArea"" class=""form-control"" value=""@b.BaseArea"" />
                                                </div>
                                                <div class=""col-6 col-lg-2"">
                                                    <label class=""form-label small"">Bodrum Kat Sayısı</label>
                                                    <input type=""number"" name=""Blocks[@i].BasementFloors"" class=""form-control"" value=""@b.BasementFloors"" />
                                                </div>
                                                <div class=""col-6 col-lg-2"">
                                                    <label class=""form-label small"">Normal Kat Sayısı</label>
                                                    <input type=""number"" name=""Blocks[@i].TotalFloors"" class=""form-control"" value=""@b.TotalFloors"" />
                                                </div>
                                                <div class=""col-6 col-lg-2"">
                                                    <label class=""form-label small"">Çatı / Teras</label>
                                                    <select name=""Blocks[@i].HasRoof"" class=""form-select"">
                                                        <option value=""true"" selected=""@(b.HasRoof)"">Var (Çatılı)</option>
                                                        <option value=""false"" selected=""@(!b.HasRoof)"">Yok (Düz)</option>
                                                    </select>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    i++;
                                }
                            }
                        </div>";

        code = code.Replace(oldBlocksContainer, newBlocksContainer);
        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Fixed blocksContainer with Razor.");
    }
}
