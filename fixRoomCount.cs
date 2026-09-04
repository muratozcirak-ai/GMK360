using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string text = File.ReadAllText(path, Encoding.UTF8);

        string oldSelects = @"                        <div class=""col-md-3 mb-3"">
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
                        </div>";
                        
        string newSelects = @"                        <div class=""col-md-3 mb-3"">
                            <label class=""form-label fw-bold small"">Oda Sayısı / Tipi</label>
                            <input class=""form-control"" name=""RoomCount"" list=""roomCountOptions"" value=""3+1"" placeholder=""Örn: 3+2"" />
                            <datalist id=""roomCountOptions"">
                                <option value=""-"">Yok (Dükkan)</option>
                                <option value=""1+1""></option>
                                <option value=""2+1""></option>
                                <option value=""3+1""></option>
                                <option value=""4+1""></option>
                                <option value=""3+2""></option>
                                <option value=""4+2""></option>
                                <option value=""Özel Dubleks""></option>
                            </datalist>
                        </div>";

        if (text.Contains(oldSelects))
        {
            text = text.Replace(oldSelects, newSelects);
            File.WriteAllText(path, text, new UTF8Encoding(true));
            Console.WriteLine("RoomCount changed to Datalist Input");
        }
        else
        {
            Console.WriteLine("Old select not found");
        }
    }
}
