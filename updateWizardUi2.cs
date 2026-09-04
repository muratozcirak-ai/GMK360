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

        // Find the start of the blockHtml definition
        int start = view.IndexOf("const blockHtml =");
        int end = view.IndexOf("$('#blocksContainer').append(blockHtml);", start);

        if (start != -1 && end != -1)
        {
            string newTemplate = @"const blockHtml = `
                          <div class=""card mb-4 border-0 shadow-sm rounded-4 block-item"">
                              <div class=""card-body p-4"">
                                  <h5 class=""fw-bold text-navy border-bottom pb-2 mb-3""><i class=""ph ph-building me-2 text-orange""></i> ${i+1}. Yapı / Blok Tanımı</h5>
                                  <div class=""row g-3"">
                                      <div class=""col-md-12 col-lg-3"">
                                          <label class=""form-label small fw-bold"">Yapı / Blok Adı</label>
                                          <input type=""text"" name=""Blocks[${i}].BlockName"" class=""form-control b-name"" value=""${defaultName}"" required ${safeCount === 1 ? 'readonly' : ''} />
                                      </div>
                                      <div class=""col-12 col-lg-2"">
                                          <label class=""form-label small fw-bold"">Taban (m²)</label>
                                          <input type=""number"" name=""Blocks[${i}].BaseArea"" class=""form-control"" placeholder=""Örn: 200"" min=""1"" required />
                                      </div>
                                      <div class=""col-6 col-lg-2"">
                                          <label class=""form-label small fw-bold"">Normal Kat Sayısı</label>
                                          <input type=""number"" name=""Blocks[${i}].TotalFloors"" class=""form-control b-floors"" value=""5"" min=""0"" required />
                                      </div>
                                      <div class=""col-6 col-lg-2"">
                                          <label class=""form-label small fw-bold"">Bodrum Kat</label>
                                          <input type=""number"" name=""Blocks[${i}].BasementFloors"" class=""form-control b-basements"" value=""1"" min=""0"" required />
                                      </div>
                                      <div class=""col-12 col-lg-3 d-flex align-items-end"">
                                          <div class=""form-check mb-2 me-4"">
                                              <input class=""form-check-input"" type=""checkbox"" name=""Blocks[${i}].HasGroundFloor"" value=""true"" id=""ground_${i}"" checked>
                                              <label class=""form-check-label small fw-bold"" for=""ground_${i}"">Zemin Kat</label>
                                          </div>
                                          <div class=""form-check mb-2"">
                                              <input class=""form-check-input"" type=""checkbox"" name=""Blocks[${i}].HasRoof"" value=""true"" id=""roof_${i}"">
                                              <label class=""form-check-label small fw-bold"" for=""roof_${i}"">Çatı Katı</label>
                                          </div>
                                      </div>
                                  </div>
                              </div>
                          </div>
                      `;
                      ";

            view = view.Substring(0, start) + newTemplate + view.Substring(end);
            File.WriteAllText(viewPath, view, new UTF8Encoding(true));
            Console.WriteLine("Create.cshtml template updated successfully.");
        }
        else
        {
            Console.WriteLine($"Could not find JS template in Create.cshtml. start={start} end={end}");
        }
    }
}
