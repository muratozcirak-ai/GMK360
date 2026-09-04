using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string text = File.ReadAllText(path, Encoding.UTF8);

        // Update display card
        string oldCardFields = @"                <div class=""mb-3"">
                    <span class=""text-muted small d-block"">Teknik Özellikler</span>
                    <span class=""fw-bold text-dark"">@(string.IsNullOrEmpty(Model.TechnicalFeatures) ? ""Belirtilmedi"" : Model.TechnicalFeatures)</span>
                </div>";
                
        string newCardFields = @"                <div class=""mb-3"">
                    <span class=""text-muted small d-block"">Mantolama / Yalıtım</span>
                    <span class=""fw-bold text-dark"">@(string.IsNullOrEmpty(Model.InsulationType) ? ""Belirtilmedi"" : Model.InsulationType)</span>
                </div>
                <div class=""row mb-3"">
                    <div class=""col-6"">
                        <span class=""text-muted small d-block"">Asansör</span>
                        <span class=""fw-bold text-dark"">@(Model.ElevatorCount.HasValue ? Model.ElevatorCount.Value.ToString() + "" Adet"" : ""Belirtilmedi"")</span>
                    </div>
                    <div class=""col-6"">
                        <span class=""text-muted small d-block"">Otopark</span>
                        <span class=""fw-bold text-dark"">@(string.IsNullOrEmpty(Model.ParkingType) ? ""Belirtilmedi"" : Model.ParkingType)</span>
                    </div>
                </div>
                <div class=""mb-3"">
                    <span class=""text-muted small d-block"">Diğer Teknik Özellikler</span>
                    <span class=""fw-bold text-dark"">@(string.IsNullOrEmpty(Model.TechnicalFeatures) ? ""Belirtilmedi"" : Model.TechnicalFeatures)</span>
                </div>";
                
        if (text.Contains(oldCardFields)) {
            text = text.Replace(oldCardFields, newCardFields);
        }

        // Update Modal
        string oldModalFields = @"                    <div class=""mb-3"">
                        <label class=""form-label fw-bold small"">Teknik Özellikler</label>
                        <textarea name=""TechnicalFeatures"" class=""form-control"" rows=""2"" placeholder=""Örn: 2 Asansör, Yük Asansörü, Çift Merdiven..."">@Model.TechnicalFeatures</textarea>
                    </div>";
                    
        string newModalFields = @"                    <div class=""row"">
                        <div class=""col-md-4 mb-3"">
                            <label class=""form-label fw-bold small"">Mantolama</label>
                            <input type=""text"" name=""InsulationType"" class=""form-control"" placeholder=""Örn: Taş Yünü"" value=""@Model.InsulationType"" />
                        </div>
                        <div class=""col-md-4 mb-3"">
                            <label class=""form-label fw-bold small"">Asansör Sayısı</label>
                            <input type=""number"" name=""ElevatorCount"" class=""form-control"" value=""@Model.ElevatorCount"" />
                        </div>
                        <div class=""col-md-4 mb-3"">
                            <label class=""form-label fw-bold small"">Otopark</label>
                            <select name=""ParkingType"" class=""form-select"">
                                <option value="""" selected=""@(string.IsNullOrEmpty(Model.ParkingType) ? ""selected"" : null)"">Belirtilmedi</option>
                                <option value=""Açık Otopark"" selected=""@(Model.ParkingType == ""Açık Otopark"" ? ""selected"" : null)"">Açık Otopark</option>
                                <option value=""Kapalı Otopark"" selected=""@(Model.ParkingType == ""Kapalı Otopark"" ? ""selected"" : null)"">Kapalı Otopark</option>
                                <option value=""Açık & Kapalı"" selected=""@(Model.ParkingType == ""Açık & Kapalı"" ? ""selected"" : null)"">Açık & Kapalı</option>
                                <option value=""Yok"" selected=""@(Model.ParkingType == ""Yok"" ? ""selected"" : null)"">Yok</option>
                            </select>
                        </div>
                    </div>
                    <div class=""mb-3"">
                        <label class=""form-label fw-bold small"">Diğer Teknik Özellikler</label>
                        <textarea name=""TechnicalFeatures"" class=""form-control"" rows=""2"" placeholder=""Eklemek istediğiniz diğer özellikler..."">@Model.TechnicalFeatures</textarea>
                    </div>";

        if (text.Contains(oldModalFields)) {
            text = text.Replace(oldModalFields, newModalFields);
        }

        File.WriteAllText(path, text, new UTF8Encoding(true));
    }
}
