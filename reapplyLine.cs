using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        var lines = new List<string>(File.ReadAllLines(path, Encoding.UTF8));

        // 1. Remove Hızlı Üretici
        int hStart = -1, hEnd = -1;
        for(int i = 0; i < lines.Count; i++) {
            if(lines[i].Contains("<!-- Otomatik")) hStart = i;
            if(hStart != -1 && lines[i].Contains("<!-- Blok D")) {
                hEnd = i;
                break;
            }
        }
        if(hStart != -1 && hEnd != -1) {
            lines.RemoveRange(hStart, hEnd - hStart);
            Console.WriteLine("Hızlı Uretici Removed");
        }

        // 2. Update Sidebar
        int sStart = -1, sEnd = -1;
        for(int i = 0; i < lines.Count; i++) {
            if(lines[i].Contains("text-muted") && lines[i].Contains("Cephe")) {
                sStart = i - 1; // get <div class="mb-3">
            }
            if(sStart != -1 && i > sStart && lines[i].Contains("text-muted") && (lines[i].Contains("Açıklama") || lines[i].Contains("Aklama"))) {
                sEnd = i - 1; // get <div>
                break;
            }
        }
        if(sStart != -1 && sEnd != -1) {
            string newSidebar = @"                <div class=""mb-3"">
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
            lines.RemoveRange(sStart, sEnd - sStart);
            lines.Insert(sStart, newSidebar);
            Console.WriteLine("Sidebar Updated");
        }

        // 3. Update Modal
        int mStart = -1, mEnd = -1;
        for(int i = 0; i < lines.Count; i++) {
            if(lines[i].Contains("name=\"FacadeDirection\"")) {
                mStart = i - 3; // go back to <div class="col-md-6 mb-3">
            }
            if(mStart != -1 && i > mStart && lines[i].Contains("name=\"Description\"")) {
                mEnd = i - 2; // go back before <div class="mb-3">
                break;
            }
        }
        if(mStart != -1 && mEnd != -1) {
            string newModal = @"                        <div class=""col-md-6 mb-3"">
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
                    </div>";
                    
            lines.RemoveRange(mStart, mEnd - mStart);
            lines.Insert(mStart, newModal);
            Console.WriteLine("Modal Updated");
        }

        File.WriteAllLines(path, lines, new UTF8Encoding(true));
    }
}
