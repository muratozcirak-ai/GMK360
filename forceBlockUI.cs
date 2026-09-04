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
        int startQuickGen = text.IndexOf("<!-- Otomatik");
        int endQuickGen = text.IndexOf("<!-- Blok Dış Cephe Renderları -->");
        if (startQuickGen != -1 && endQuickGen != -1 && endQuickGen > startQuickGen)
        {
            text = text.Remove(startQuickGen, endQuickGen - startQuickGen);
            Console.WriteLine("Quick Generator Removed");
        }
        else
        {
            // Try with Turkish char variants
            startQuickGen = text.IndexOf("<!-- Otomatik");
            endQuickGen = text.IndexOf("<!-- Blok D");
            if (startQuickGen != -1 && endQuickGen != -1 && endQuickGen > startQuickGen)
            {
                text = text.Remove(startQuickGen, endQuickGen - startQuickGen);
                Console.WriteLine("Quick Generator Removed (variant)");
            }
        }

        // 2. Update Sidebar
        int startSidebar = text.IndexOf(@"<div class=""mb-3"">
                    <span class=""text-muted small d-block"">Cephe</span>");
        int endSidebar = text.IndexOf(@"<div>
                    <span class=""text-muted small d-block"">Açıklama</span>");
                    
        if (startSidebar == -1) 
        {
            // Turkish encoding variant
            startSidebar = text.IndexOf(@"<span class=""text-muted small d-block"">Cephe</span>");
            if(startSidebar != -1) startSidebar -= 40; // go back to <div class="mb-3">
            endSidebar = text.IndexOf(@"<span class=""text-muted small d-block"">A");
            if(endSidebar != -1) endSidebar -= 20; // go back to <div>
        }

        if (startSidebar != -1 && endSidebar != -1 && endSidebar > startSidebar)
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
            text = text.Remove(startSidebar, endSidebar - startSidebar);
            text = text.Insert(startSidebar, newSidebar);
            Console.WriteLine("Sidebar Updated");
        }
        
        // 3. Update Modal
        int startModal = text.IndexOf(@"<div class=""col-md-6 mb-3"">
                            <label class=""form-label fw-bold small"">Cephe</label>");
        if(startModal == -1) startModal = text.IndexOf(@"<label class=""form-label fw-bold small"">Cephe</label>");
        if(startModal != -1) startModal -= 50;

        int endModal = text.IndexOf(@"</div>
                </div>
                <div class=""modal-footer border-0 pt-0"">");
        
        if (startModal != -1 && endModal != -1 && endModal > startModal)
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
                    ";
                    
            text = text.Remove(startModal, endModal - startModal);
            text = text.Insert(startModal, newModal);
            Console.WriteLine("Modal Updated");
        }

        File.WriteAllText(path, text, new UTF8Encoding(true));
    }
}
