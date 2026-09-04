using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string text = File.ReadAllText(path, Encoding.UTF8);

        // 1. Remove Quick Generator
        int quickGenStart = text.IndexOf("<!-- Otomatik");
        int quickGenEnd = text.IndexOf("<!-- Blok D");
        if(quickGenStart != -1 && quickGenEnd != -1)
        {
            text = text.Remove(quickGenStart, quickGenEnd - quickGenStart);
            Console.WriteLine("Removed Quick Generator safely.");
        }

        // 2. Update Sidebar
        string targetSidebarStart = @"<div class=""mb-3"">
                    <span class=""text-muted small d-block"">Cephe</span>";
        int sidebarIdx = text.IndexOf(targetSidebarStart);
        if (sidebarIdx != -1)
        {
            int sidebarEnd = text.IndexOf(@"<div>
                    <span class=""text-muted small d-block"">A", sidebarIdx);
            if(sidebarEnd != -1)
            {
                string newSidebar = @"<div class=""mb-3"">
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
                </div>
                ";
                text = text.Remove(sidebarIdx, sidebarEnd - sidebarIdx);
                text = text.Insert(sidebarIdx, newSidebar);
                Console.WriteLine("Sidebar Updated safely.");
            }
        }
        else
        {
            Console.WriteLine("Could not find sidebar target (encoding issues?). Trying fallback.");
            int altStart = text.IndexOf(@"<span class=""text-muted small d-block"">Cephe</span>");
            if (altStart != -1)
            {
                altStart = text.LastIndexOf("<div", altStart);
                int altEnd = text.IndexOf(@"<span class=""text-muted small d-block"">A", altStart);
                if (altEnd != -1) altEnd = text.LastIndexOf("<div", altEnd);
                
                string newSidebar = @"<div class=""mb-3"">
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
                </div>
                ";
                text = text.Remove(altStart, altEnd - altStart);
                text = text.Insert(altStart, newSidebar);
                Console.WriteLine("Sidebar Updated via fallback.");
            }
        }

        // 3. Update Modal
        int modalStart = text.IndexOf(@"<div class=""col-md-6 mb-3"">
                            <label class=""form-label fw-bold small"">Cephe</label>");
        if(modalStart == -1)
        {
             modalStart = text.IndexOf(@"<label class=""form-label fw-bold small"">Cephe</label>");
             if(modalStart != -1) modalStart = text.LastIndexOf("<div", modalStart);
        }

        if(modalStart != -1)
        {
            // Find end of TechnicalFeatures div
            int modalEnd = text.IndexOf(@"name=""Description""", modalStart);
            if(modalEnd != -1) modalEnd = text.LastIndexOf("<div", modalEnd);
            
            if(modalEnd != -1)
            {
                string newModal = @"<div class=""col-md-6 mb-3"">
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
                    
                    <div class=""mb-4"">
                        <label class=""form-label fw-bold small text-primary""><i class=""bi bi-ui-checks-grid me-1""></i> Diğer Donanım ve Altyapılar</label>
                        <div class=""row g-2"">
                            @{
                                var existingFeatures = Model.TechnicalFeatures ?? """";
                                string[] predefined = { ""Su Deposu"", ""Sığınak"", ""Jeneratör"", ""Fiber İnternet"", ""Uydu Sistemi"", ""Görüntülü Diyafon"", ""Şifreli Giriş"", ""Kamera Sistemi"", ""Akıllı Ev Altyapısı"", ""Engelli Rampası"" };
                            }
                            @foreach(var item in predefined)
                            {
                                <div class=""col-md-4 col-sm-6"">
                                    <div class=""form-check form-check-custom form-check-solid"">
                                        <input class=""form-check-input"" type=""checkbox"" name=""SelectedFeatures"" value=""@item"" id=""feat_@item.Replace("" "", """")"" @(existingFeatures.Contains(item) ? ""checked"" : """") />
                                        <label class=""form-check-label text-dark small"" for=""feat_@item.Replace("" "", """")"">@item</label>
                                    </div>
                                </div>
                            }
                        </div>
                    </div>
                    <div class=""mb-3"">
                        <label class=""form-label fw-bold small"">Özel Eklemeler ve Notlar</label>
                        <div class=""form-text mb-2"">Yukarıdaki listede olmayan ekstra bir özellik varsa (örn: Güneş Paneli) veya detay yazmak istiyorsanız buraya ekleyebilirsiniz.</div>
                        <textarea name=""TechnicalFeatures"" class=""form-control"" rows=""2""></textarea>
                    </div>
                    ";
                    
                text = text.Remove(modalStart, modalEnd - modalStart);
                text = text.Insert(modalStart, newModal);
                Console.WriteLine("Modal Updated safely.");
            }
        }
        else
        {
            Console.WriteLine("Could not find modal start.");
        }

        File.WriteAllText(path, text, new UTF8Encoding(true));
    }
}
