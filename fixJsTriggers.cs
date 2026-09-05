using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string js1 = @"if ($('#CityId').val()) { $('#CityId').trigger('change'); }";
        string js2 = @"if ($('#DistrictId').val()) { $('#DistrictId').trigger('change'); }";
        string js3 = @"if ($('#NeighborhoodId').val()) { $('#NeighborhoodId').trigger('change'); }";

        code = code.Replace(js1, "// " + js1);
        code = code.Replace(js2, "// " + js2);
        code = code.Replace(js3, "// " + js3);

        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Fixed JS triggers.");
    }
}
