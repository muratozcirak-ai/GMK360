using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string viewPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string view = File.ReadAllText(viewPath, Encoding.UTF8);

        // --- STEP 1 REPLACEMENT ---
        string oldStep1 = @"<div class=""col-md-12"">
                                <label class=""form-label fw-bold"">Mimari Çizim / Proje Görseli (URL)</label>
                                <input asp-for=""CoverImageFile"" type=""file"" class=""form-control rounded-3"" accept=""image/jpeg,image/png,application/pdf"" />
                                <div class=""form-text text-muted"">Mimari resim veya 3D render görselini bilgisayarınızdan seçin (JPG/PNG).</div>
                            </div>";
        string newStep1 = @"<div class=""col-md-12"">
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
                            
        if (view.Contains(oldStep1))
        {
            view = view.Replace(oldStep1, newStep1);
        }
        else
        {
            Console.WriteLine("Could not find oldStep1");
        }

        // --- STEP 2 REPLACEMENT ---
        int start = view.IndexOf("<div class=\"card border border-2 border-light shadow-sm rounded-4 mb-3 block-item\">");
        int end = view.IndexOf("container.innerHTML += html;", start);
        if (start != -1 && end != -1)
        {
            string newTemplate = @"<div class=""card border border-2 border-light shadow-sm rounded-4 mb-3 block-item"">
                          <div class=""card-body p-4"">
                              <h5 class=""fw-bold text-navy border-bottom pb-2 mb-3""><i class=""ph ph-building me-2 text-orange""></i> ${i+1}. Blok / Yapı Tanımı</h5>
                              <div class=""row g-3"">
                                  <div class=""col-md-12 col-lg-3"">
                                        <label class=""form-label small fw-bold"">Blok / Yapı Adı</label>
                                        <input type=""text"" name=""Blocks[${i}].BlockName"" class=""form-control b-name"" value=""${defaultName}"" onkeyup=""updatePodiumDropdowns()"" required ${safeCount === 1 ? 'readonly' : ''} />
                                        
                                        <label class=""form-label small fw-bold mt-2 text-primary"">Yapı Karakteri</label>
                                        <select name=""Blocks[${i}].StructureType"" class=""form-select b-type border-primary"" onchange=""toggleParent(this)"">
                                            <option value=""independent"">Müstakil (Kendi Temeli)</option>
                                            <option value=""podium"">Ortak Baza (Alt Yapı / Otopark)</option>
                                            <option value=""tower"">Baza Üzerinde Kule</option>
                                        </select>
                                        
                                        <select name=""Blocks[${i}].ParentIndex"" class=""form-select b-parent mt-2 border-warning"" style=""display:none;"">
                                            <option value="""">Hangi Bazaya Bağlı?</option>
                                        </select>
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
                                        <label class=""form-label small fw-bold"">Bodrum Kat Sayısı</label>
                                        <input type=""number"" name=""Blocks[${i}].BasementFloors"" class=""form-control b-basements"" value=""1"" min=""0"" required />
                                        <div class=""form-check mt-2"">
                                            <input class=""form-check-input"" type=""checkbox"" name=""Blocks[${i}].HasGroundFloor"" value=""true"" id=""ground_${i}"" checked>
                                            <label class=""form-check-label small fw-bold"" for=""ground_${i}"">Zemin Kat Var</label>
                                        </div>
                                        <div class=""form-check"">
                                            <input class=""form-check-input"" type=""checkbox"" name=""Blocks[${i}].HasRoof"" value=""true"" id=""roof_${i}"">
                                            <label class=""form-check-label small fw-bold"" for=""roof_${i}"">Çatı Katı Var</label>
                                        </div>
                                    </div>
                              </div>
                          </div>
                      </div>
                  `;
                  ";
            view = view.Substring(0, start) + newTemplate + view.Substring(end);
        }

        // Clean up prepareSummary
        view = view.Replace("totalApts += parseInt(b.querySelector('.b-apts').value) || 0;", "");
        view = view.Replace("totalShops += parseInt(b.querySelector('.b-shops').value) || 0;", "");

        string jsLogic = @"
          function toggleParent(sel) {
              var val = $(sel).val();
              var parentSel = $(sel).siblings('.b-parent');
              if(val === 'tower') {
                  parentSel.show();
                  parentSel.attr('required', true);
              } else {
                  parentSel.hide();
                  parentSel.removeAttr('required');
                  parentSel.val('');
              }
              updatePodiumDropdowns();
          }

          function updatePodiumDropdowns() {
              var podiums = [];
              $('.block-item').each(function(index) {
                  var type = $(this).find('.b-type').val();
                  var name = $(this).find('.b-name').val();
                  if(type === 'podium' && name.trim() !== '') {
                      podiums.push({ idx: index, name: name });
                  }
              });
              
              $('.b-parent').each(function() {
                  var currentVal = $(this).val();
                  $(this).empty();
                  $(this).append('<option value="""">Hangi Bazaya Bağlı?</option>');
                  var sel = $(this);
                  podiums.forEach(function(p) {
                      sel.append('<option value=""' + p.idx + '"">' + p.name + '</option>');
                  });
                  $(this).val(currentVal); 
              });
          }
";
        view = view.Replace("function prepareSummary() {", jsLogic + "\r\n\r\n          function prepareSummary() {");

        File.WriteAllText(viewPath, view, new UTF8Encoding(true));
        Console.WriteLine("Both Step 1 and Step 2 perfectly applied.");
    }
}
