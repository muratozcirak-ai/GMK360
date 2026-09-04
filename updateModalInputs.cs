using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string text = File.ReadAllText(path, Encoding.UTF8);

        string oldSelects = @"                        <div class=""col-md-6 mb-3"">
                            <label class=""form-label fw-bold small"">Birim Tipi / Plan</label>
                            <select name=""RoomLayout"" class=""form-select"">
                                <option value=""3+1"" selected>3+1 Daire</option>
                                <option value=""2+1"">2+1 Daire</option>
                                <option value=""1+1"">1+1 Daire</option>
                                <option value=""4+1"">4+1 Daire</option>
                                <option value=""Dükkan"">Dükkan / Ticari</option>
                                <option value=""Depo"">Depo</option>
                            </select>
                        </div>";
                        
        string newSelects = @"                        <div class=""col-md-3 mb-3"">
                            <label class=""form-label fw-bold small"">Oda Sayısı</label>
                            <select name=""RoomCount"" class=""form-select"">
                                <option value=""-"">Yok (Dükkan)</option>
                                <option value=""1+0"">1+0 (Stüdyo)</option>
                                <option value=""1+1"">1+1</option>
                                <option value=""2+1"">2+1</option>
                                <option value=""3+1"" selected>3+1</option>
                                <option value=""4+1"">4+1</option>
                                <option value=""5+1"">5+1</option>
                                <option value=""5+2"">5+2</option>
                            </select>
                        </div>
                        <div class=""col-md-3 mb-3"">
                            <label class=""form-label fw-bold small"">Yapı / Tür</label>
                            <select name=""UnitStructure"" class=""form-select"">
                                <option value=""Ara Kat"" selected>Ara Kat</option>
                                <option value=""Çatı Dubleksi"">Çatı Dubleksi</option>
                                <option value=""Ters Dubleks"">Ters Dubleks</option>
                                <option value=""Bahçe Katı"">Bahçe Katı</option>
                                <option value=""Dükkan / Ticari"">Dükkan / Ticari</option>
                                <option value=""Depo"">Depo</option>
                            </select>
                        </div>";

        string formStart = @"<form id=""addUnitForm"" action=""#"" method=""post"">";
        string formNewStart = @"<form id=""addUnitForm"" asp-action=""AddBuildingUnit"" method=""post"">";
        
        string oldBtn = @"<button type=""button"" class=""btn btn-success rounded-pill px-4"" onclick=""alert('C# Ekleme Action\'ı az sonra yazılacak!')"">Bölümü Ekle</button>";
        string newBtn = @"<button type=""submit"" class=""btn btn-success rounded-pill px-4"">Bölümü Ekle</button>";

        bool changed = false;
        
        if (text.Contains(oldSelects))
        {
            text = text.Replace(oldSelects, newSelects);
            changed = true;
        }
        
        if (text.Contains(formStart))
        {
            text = text.Replace(formStart, formNewStart);
            changed = true;
        }
        
        if (text.Contains(oldBtn))
        {
            text = text.Replace(oldBtn, newBtn);
            changed = true;
        }

        if (changed)
        {
            File.WriteAllText(path, text, new UTF8Encoding(true));
            Console.WriteLine("Modal Form Updated for Unit Structure");
        }
    }
}
