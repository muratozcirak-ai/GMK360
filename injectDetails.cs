using System;
using System.IO;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string text = File.ReadAllText(path);

        string target = "<!-- Otomatik Üretim Sihirbazı -->";
        
        string blockDetailsCard = @"
        <!-- Blok Özellikleri -->
        <div class=""card border-0 shadow-sm rounded-4 mb-4"">
            <div class=""card-body p-4"">
                <div class=""d-flex justify-content-between align-items-center border-bottom pb-2 mb-3"">
                    <h5 class=""fw-bold mb-0""><i class=""bi bi-info-square text-info me-2""></i>Blok Özellikleri</h5>
                    <button class=""btn btn-sm btn-icon btn-light-primary rounded-circle"" data-bs-toggle=""modal"" data-bs-target=""#editBlockDetailsModal"" title=""Düzenle"">
                        <i class=""bi bi-pencil""></i>
                    </button>
                </div>
                <div class=""mb-3"">
                    <span class=""text-muted small d-block"">Taban Oturumu</span>
                    <span class=""fw-bold text-dark"">@(Model.BaseArea.HasValue ? Model.BaseArea.Value + "" m²"" : ""Belirtilmedi"")</span>
                </div>
                <div class=""mb-3"">
                    <span class=""text-muted small d-block"">Cephe</span>
                    <span class=""fw-bold text-dark"">@(string.IsNullOrEmpty(Model.FacadeDirection) ? ""Belirtilmedi"" : Model.FacadeDirection)</span>
                </div>
                <div class=""mb-3"">
                    <span class=""text-muted small d-block"">Teknik Özellikler</span>
                    <span class=""fw-bold text-dark"">@(string.IsNullOrEmpty(Model.TechnicalFeatures) ? ""Belirtilmedi"" : Model.TechnicalFeatures)</span>
                </div>
                <div>
                    <span class=""text-muted small d-block"">Açıklama</span>
                    <span class=""text-dark small"">@(string.IsNullOrEmpty(Model.Description) ? ""Belirtilmedi"" : Model.Description)</span>
                </div>
            </div>
        </div>

";

        string modalHtml = @"
<!-- Edit Block Details Modal -->
<div class=""modal fade"" id=""editBlockDetailsModal"" tabindex=""-1"">
    <div class=""modal-dialog"">
        <div class=""modal-content rounded-4 border-0 shadow"">
            <form asp-action=""UpdateBlockDetails"" method=""post"">
                <input type=""hidden"" name=""Id"" value=""@Model.Id"" />
                <div class=""modal-header border-0 pb-0"">
                    <h5 class=""modal-title fw-bold""><i class=""bi bi-pencil-square text-primary me-2""></i>Blok Özelliklerini Düzenle</h5>
                    <button type=""button"" class=""btn-close"" data-bs-dismiss=""modal""></button>
                </div>
                <div class=""modal-body"">
                    <div class=""mb-3"">
                        <label class=""form-label fw-bold small"">Blok Adı</label>
                        <input type=""text"" name=""BlockName"" class=""form-control"" value=""@Model.BlockName"" required />
                    </div>
                    <div class=""row"">
                        <div class=""col-md-6 mb-3"">
                            <label class=""form-label fw-bold small"">Taban Oturumu (m²)</label>
                            <input type=""number"" step=""0.1"" name=""BaseArea"" class=""form-control"" value=""@Model.BaseArea"" />
                        </div>
                        <div class=""col-md-6 mb-3"">
                            <label class=""form-label fw-bold small"">Cephe</label>
                            <input type=""text"" name=""FacadeDirection"" class=""form-control"" placeholder=""Örn: Güney-Batı"" value=""@Model.FacadeDirection"" />
                        </div>
                    </div>
                    <div class=""mb-3"">
                        <label class=""form-label fw-bold small"">Teknik Özellikler</label>
                        <textarea name=""TechnicalFeatures"" class=""form-control"" rows=""2"" placeholder=""Örn: 2 Asansör, Yük Asansörü, Çift Merdiven..."">@Model.TechnicalFeatures</textarea>
                    </div>
                    <div class=""mb-3"">
                        <label class=""form-label fw-bold small"">Genel Açıklama</label>
                        <textarea name=""Description"" class=""form-control"" rows=""2"">@Model.Description</textarea>
                    </div>
                </div>
                <div class=""modal-footer border-0 pt-0"">
                    <button type=""button"" class=""btn btn-light rounded-pill px-4"" data-bs-dismiss=""modal"">İptal</button>
                    <button type=""submit"" class=""btn btn-primary rounded-pill px-4"">Kaydet</button>
                </div>
            </form>
        </div>
    </div>
</div>
";

        if(text.Contains(target) && !text.Contains("Blok Özellikleri"))
        {
            text = text.Replace(target, blockDetailsCard + target);
            text += Environment.NewLine + modalHtml;
            File.WriteAllText(path, text, System.Text.Encoding.UTF8);
            Console.WriteLine("Successfully added UI.");
        }
        else
        {
            Console.WriteLine("Target not found or already injected.");
        }
        
        // Fix the overlapping badge issue on the right side header
        string targetBadge = @"<div class=""d-flex justify-content-between align-items-center border-bottom pb-2 mb-4"">
                    <h5 class=""fw-bold mb-0""><i class=""bi bi-diagram-3 text-navy me-2""></i>Mimari Ağaç ve Bağımsız Bölümler</h5>
                    <span class=""badge bg-primary rounded-pill px-3 py-2"">Toplam @(Model.Units?.Count ?? 0) Adet</span>
                </div>";
        string fixedBadge = @"<div class=""d-flex justify-content-between align-items-center border-bottom pb-2 mb-4"">
                    <h5 class=""fw-bold mb-0 d-flex align-items-center""><i class=""bi bi-diagram-3 text-navy me-2""></i>Ağaç ve Bağımsız Bölümler</h5>
                    <span class=""badge bg-primary rounded-pill px-3 py-2 ms-3 flex-shrink-0"">Toplam @(Model.Units?.Count ?? 0) Adet</span>
                </div>";
        
        if (text.Contains("Toplam @(Model.Units?.Count ?? 0) Adet"))
        {
             // Simple string replace for the badge overlapping
             text = text.Replace(@"<span class=""badge bg-primary rounded-pill px-3 py-2"">Toplam @(Model.Units?.Count ?? 0) Adet</span>", 
                                 @"<span class=""badge bg-primary rounded-pill px-3 py-2 ms-3 flex-shrink-0"">Toplam @(Model.Units?.Count ?? 0) Adet</span>");
             File.WriteAllText(path, text, System.Text.Encoding.UTF8);
        }
    }
}
