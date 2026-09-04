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
        int startHzl = -1;
        int endHzl = -1;
        for(int i = 0; i < lines.Count; i++)
        {
            if(lines[i].Contains("<!-- Otomatik") || lines[i].Contains("Hzl Kat/Daire")) 
            {
                if(startHzl == -1) startHzl = i;
            }
            if(lines[i].Contains("<!-- Mimari") || lines[i].Contains("<!-- Blok D"))
            {
                if(startHzl != -1 && endHzl == -1) endHzl = i;
            }
        }
        
        if(startHzl != -1 && endHzl != -1)
        {
            // Traverse backwards to include the card div if startHzl matched the comment
            while(startHzl > 0 && !lines[startHzl].Contains("<div class=\"col-md-3\">"))
                startHzl--;
            startHzl++; // leave col-md-3

            lines.RemoveRange(startHzl, endHzl - startHzl);
            Console.WriteLine("Removed Hızlı Uretici");
        }

        // 2. Fix Modal (we need to search again because line numbers shifted)
        int startModal = -1;
        int endModal = -1;
        for(int i = 0; i < lines.Count; i++)
        {
            if(lines[i].Contains(@"name=""BlockName"""))
            {
                startModal = i + 2; // skip the div closing
            }
            if(startModal != -1 && i > startModal && lines[i].Contains(@"name=""Description"""))
            {
                endModal = i - 1; // go back before the description div
                break;
            }
        }

        if(startModal != -1 && endModal != -1)
        {
            string newModal = @"
                    <div class=""row"">
                        <div class=""col-md-6 mb-3"">
                            <label class=""form-label fw-bold small"">Taban Oturumu (m²)</label>
                            <input type=""number"" step=""0.1"" name=""BaseArea"" class=""form-control"" value=""@Model.BaseArea"" />
                        </div>
                        <div class=""col-md-6 mb-3"">
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

            lines.RemoveRange(startModal, endModal - startModal);
            lines.Insert(startModal, newModal);
            Console.WriteLine("Modal Fixed");
        }

        File.WriteAllLines(path, lines, new UTF8Encoding(true));
    }
}
