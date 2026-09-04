using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string text = File.ReadAllText(path, Encoding.UTF8);

        // 1. Update UnitStructure dropdown
        string oldStructure = @"                            <select name=""UnitStructure"" class=""form-select"">
                                <option value=""Ara Kat"" selected>Ara Kat</option>
                                <option value=""Çatı Dubleksi"">Çatı Dubleksi</option>
                                <option value=""Ters Dubleks"">Ters Dubleks</option>
                                <option value=""Bahçe Katı"">Bahçe Katı</option>
                                <option value=""Dükkan / Ticari"">Dükkan / Ticari</option>
                                <option value=""Depo"">Depo</option>
                            </select>";
                            
        string newStructure = @"                            <select name=""UnitStructure"" class=""form-select"">
                                <option value=""Ara Kat"" selected>Ara Kat</option>
                                <option value=""Çatı Dubleksi"">Çatı Dubleksi</option>
                                <option value=""Ters Dubleks"">Ters Dubleks</option>
                                <option value=""Bahçe Katı"">Bahçe Katı</option>
                                <option value=""Dükkan / Ticari"">Dükkan / Ticari</option>
                                <option value=""Ortak Alan"">Ortak Alan (Sığınak, Kömürlük vb.)</option>
                                <option value=""Depo"">Depo</option>
                            </select>";

        // 2. Update RoomCount input default value from "3+1" to empty, let placeholder guide them
        string oldRoomCount = @"<input class=""form-control"" name=""RoomCount"" list=""roomCountOptions"" value=""3+1"" placeholder=""Örn: 3+2"" />";
        string newRoomCount = @"<input class=""form-control"" name=""RoomCount"" list=""roomCountOptions"" value="""" placeholder=""Boş bırakabilirsiniz"" />";
        
        string oldDatalist = @"<option value=""-"">Yok (Dükkan)</option>";
        string newDatalist = @"<option value=""-"">Yok (Dükkan / Ortak Alan)</option>";

        // 3. Update Upload Button
        string oldUploadBtn = @"<button type=""submit"" class=""btn btn-outline-primary""><i class=""bi bi-upload""></i></button>";
        string newUploadBtn = @"<button type=""submit"" class=""btn btn-primary""><i class=""bi bi-upload me-1""></i>Yükle</button>";

        bool changed = false;
        
        if (text.Contains(oldStructure)) { text = text.Replace(oldStructure, newStructure); changed = true; }
        if (text.Contains(oldRoomCount)) { text = text.Replace(oldRoomCount, newRoomCount); changed = true; }
        if (text.Contains(oldDatalist)) { text = text.Replace(oldDatalist, newDatalist); changed = true; }
        if (text.Contains(oldUploadBtn)) { text = text.Replace(oldUploadBtn, newUploadBtn); changed = true; }

        if (changed)
        {
            File.WriteAllText(path, text, new UTF8Encoding(true));
            Console.WriteLine("UI updated for Common Areas and Upload Button");
        }
    }
}
