using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Amenities.cshtml";
        string text = File.ReadAllText(path, Encoding.UTF8);

        // We want to replace the top card entirely with our new Math / Auto-control logic
        string newTopCard = @"
<div class=""card shadow-sm rounded-4 border-0 mb-5"">
    <div class=""card-body p-5"">
        <div class=""row g-5"">
            <!-- Sol: Toplam Arazi Girişi -->
            <div class=""col-md-5 border-end pe-md-5"">
                <h5 class=""fw-bold mb-4""><i class=""bi bi-aspect-ratio text-primary me-2""></i>Genel Arazi Bilgisi</h5>
                <form asp-action=""UpdateProjectLandArea"" method=""post"">
                    <input type=""hidden"" name=""projectId"" value=""@Model.Id"" />
                    <input type=""hidden"" name=""landscapeArea"" value="""" /> <!-- Removed landscape area -->
                    
                    <div class=""mb-4"">
                        <label class=""form-label fw-bold text-muted"">Proje Toplam Arazi Alanı (m²)</label>
                        <div class=""input-group input-group-solid"">
                            <input type=""number"" step=""0.1"" name=""totalLandArea"" class=""form-control form-control-lg fw-bolder text-dark"" value=""@Model.TotalLandArea"" placeholder=""Örn: 2500"" />
                            <span class=""input-group-text bg-light"">m²</span>
                        </div>
                        <div class=""form-text"">Tapuda geçen toplam arsa büyüklüğü.</div>
                    </div>
                    
                    <button type=""submit"" class=""btn btn-primary rounded-pill w-100 fw-bold""><i class=""bi bi-check2""></i> Araziyi Kaydet</button>
                </form>
            </div>
            
            <!-- Sağ: Matematik ve Bloklar -->
            <div class=""col-md-7 ps-md-5"">
                <h5 class=""fw-bold mb-4""><i class=""bi bi-calculator text-success me-2""></i>Arazi Dağılımı ve Oto-Kontrol</h5>
                
                @{
                    double totalLand = Model.TotalLandArea ?? 0;
                    double totalBlocksFootprint = Model.Blocks?.Sum(b => b.BaseArea ?? 0) ?? 0;
                    double totalAmenities = Model.Amenities?.Sum(a => a.SquareMeters ?? 0) ?? 0;
                    double remainingSpace = totalLand - totalBlocksFootprint - totalAmenities;
                    
                    // Progress bar math
                    double blocksPercent = totalLand > 0 ? (totalBlocksFootprint / totalLand) * 100 : 0;
                    double amenitiesPercent = totalLand > 0 ? (totalAmenities / totalLand) * 100 : 0;
                }
                
                <div class=""d-flex justify-content-between align-items-center mb-2"">
                    <span class=""fw-bold text-muted"">Blokların Toplam Taban Oturumu:</span>
                    <span class=""fw-bolder fs-5 text-dark"">@totalBlocksFootprint m²</span>
                </div>
                <!-- Blok Detayları Listesi -->
                @if(Model.Blocks != null && Model.Blocks.Any())
                {
                    <div class=""bg-light rounded p-3 mb-3"">
                        @foreach(var block in Model.Blocks)
                        {
                            <div class=""d-flex justify-content-between text-muted small mb-1"">
                                <span><i class=""bi bi-building me-1""></i> @block.Name</span>
                                <span>@(block.BaseArea.HasValue ? block.BaseArea.Value + "" m²"" : ""Belirtilmedi (Lütfen Blok ayarlarından girin)"")</span>
                            </div>
                        }
                    </div>
                }

                <div class=""d-flex justify-content-between align-items-center mb-2"">
                    <span class=""fw-bold text-muted"">Eklenen Dış Alanlar (Kamelya, Park vb):</span>
                    <span class=""fw-bolder fs-5 text-dark"">@totalAmenities m²</span>
                </div>
                
                <hr />
                
                <div class=""d-flex justify-content-between align-items-center mb-3"">
                    <span class=""fw-bolder fs-4"">Kalan Boş Alan (Net):</span>
                    <span class=""fw-bolder fs-3 @(remainingSpace < 0 ? ""text-danger"" : ""text-success"")"">@remainingSpace m²</span>
                </div>
                
                @if(totalLand > 0)
                {
                    <div class=""progress h-8px rounded-pill"">
                        <div class=""progress-bar bg-primary"" style=""width: @blocksPercent.ToString(""0.##"", System.Globalization.CultureInfo.InvariantCulture)%""></div>
                        <div class=""progress-bar bg-warning"" style=""width: @amenitiesPercent.ToString(""0.##"", System.Globalization.CultureInfo.InvariantCulture)%""></div>
                    </div>
                    <div class=""d-flex mt-2 small text-muted justify-content-between"">
                        <span><i class=""bi bi-circle-fill text-primary""></i> Binalar</span>
                        <span><i class=""bi bi-circle-fill text-warning""></i> Donatılar</span>
                        <span><i class=""bi bi-circle-fill text-light""></i> Boş</span>
                    </div>
                }
                
                @if(remainingSpace < 0)
                {
                    <div class=""alert alert-danger mt-3 mb-0 py-2"">
                        <i class=""bi bi-exclamation-triangle me-2""></i> <strong>Uyarı!</strong> Eklediğiniz binaların ve donatıların toplam alanı, genel arazi alanını aşıyor. Lütfen metrekareleri kontrol edin.
                    </div>
                }
            </div>
        </div>
    </div>
</div>
";

        // Find the previous top card we injected
        // It starts with <div class="card shadow-sm rounded-4 border-0 mb-5"> and ends right before <div class="row g-5">
        string pattern = @"<div class=""card shadow-sm rounded-4 border-0 mb-5"">.*?Arazi Değerlerini Kaydet</button>\s*</form>\s*</div>\s*</div>";
        Match m = Regex.Match(text, pattern, RegexOptions.Singleline);
        if(m.Success)
        {
            text = text.Replace(m.Value, newTopCard);
            File.WriteAllText(path, text, new UTF8Encoding(true));
            Console.WriteLine("Replaced successfully.");
        }
        else
        {
            Console.WriteLine("Could not find the old top card to replace.");
        }
    }
}
