using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string text = File.ReadAllText(path, Encoding.UTF8);

        // 1. Remove the "Hızlı Kat/Daire Üretici" Side Panel
        string quickGenPattern = @"<!-- Otomatik Üretim Sihirbazı -->\s*<div class=""card border-0 shadow-sm rounded-4 mb-4"">.*?</div>\s*</div>\s*</div>\s*<!-- Blok Dış Cephe Renderları -->";
        
        text = Regex.Replace(text, quickGenPattern, "<!-- Blok Dış Cephe Renderları -->", RegexOptions.Singleline);
        Console.WriteLine("Removed Quick Generator");

        // 2. Fix the Block Features Sidebar display
        string sidebarPattern = @"<div class=""mb-3"">\s*<span class=""text-muted small d-block"">Cephe</span>.*?<span class=""fw-bold text-dark"">@\(string\.IsNullOrEmpty\(Model\.TechnicalFeatures\) \? ""Belirtilmedi"" \: Model\.TechnicalFeatures\)</span>\s*</div>";
        
        string newSidebarFeatures = @"<div class=""mb-3"">
                    <span class=""text-muted small d-block"">Otopark</span>
                    <span class=""fw-bold text-dark"">@(string.IsNullOrEmpty(Model.ParkingType) ? ""Belirtilmedi"" : Model.ParkingType)</span>
                </div>
                <div class=""mb-3"">
                    <span class=""text-muted small d-block"">Asansör</span>
                    <span class=""fw-bold text-dark"">@(Model.ElevatorCount.HasValue ? Model.ElevatorCount + "" Adet"" : ""Belirtilmedi"")</span>
                </div>
                <div class=""mb-3"">
                    <span class=""text-muted small d-block"">Mantolama / Yalıtım</span>
                    <span class=""fw-bold text-dark"">@(string.IsNullOrEmpty(Model.InsulationType) ? ""Belirtilmedi"" : Model.InsulationType)</span>
                </div>";
                
        text = Regex.Replace(text, sidebarPattern, newSidebarFeatures, RegexOptions.Singleline);
        Console.WriteLine("Updated Sidebar Features");


        // 3. Fix the editBlockDetailsModal
        string modalPattern = @"<div class=""col-md-6 mb-3"">\s*<label class=""form-label fw-bold small"">Cephe</label>.*?<textarea name=""TechnicalFeatures"" class=""form-control"" rows=""3"">@Model\.TechnicalFeatures</textarea>\s*</div>";
        
        string newModalFeatures = @"<div class=""col-md-6 mb-3"">
                            <label class=""form-label fw-bold small"">Asansör Sayısı</label>
                            <input type=""number"" name=""ElevatorCount"" class=""form-control"" value=""@Model.ElevatorCount"" placeholder=""Örn: 2"" />
                        </div>
                    </div>
                    <div class=""row"">
                        <div class=""col-md-6 mb-3"">
                            <label class=""form-label fw-bold small"">Otopark</label>
                            <select name=""ParkingType"" class=""form-select"">
                                <option value="""" selected=""@(string.IsNullOrEmpty(Model.ParkingType))"">Seçiniz...</option>
                                <option value=""Kapalı Otopark"" selected=""@(Model.ParkingType == ""Kapalı Otopark"")"">Kapalı Otopark</option>
                                <option value=""Açık Otopark"" selected=""@(Model.ParkingType == ""Açık Otopark"")"">Açık Otopark</option>
                                <option value=""Açık ve Kapalı Otopark"" selected=""@(Model.ParkingType == ""Açık ve Kapalı Otopark"")"">Açık ve Kapalı Otopark</option>
                                <option value=""Yok"" selected=""@(Model.ParkingType == ""Yok"")"">Yok</option>
                            </select>
                        </div>
                        <div class=""col-md-6 mb-3"">
                            <label class=""form-label fw-bold small"">Mantolama / Yalıtım</label>
                            <input type=""text"" name=""InsulationType"" class=""form-control"" placeholder=""Örn: Taş Yünü"" value=""@Model.InsulationType"" />
                        </div>
                    </div>
                    <div class=""mb-3"">
                        <label class=""form-label fw-bold small"">Diğer Teknik Özellikler (Sığınak, Su Deposu vb.)</label>
                        <textarea name=""TechnicalFeatures"" class=""form-control"" rows=""2"">@Model.TechnicalFeatures</textarea>
                    </div>";
                    
        text = Regex.Replace(text, modalPattern, newModalFeatures, RegexOptions.Singleline);
        Console.WriteLine("Updated Modal Features");

        File.WriteAllText(path, text, new UTF8Encoding(true));
    }
}
