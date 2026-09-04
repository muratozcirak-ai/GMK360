using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string view = File.ReadAllText(path, Encoding.UTF8);

        // Find the "Bina / Kapı No" and "Harita Konumu" block
        string kapinoBlock = @"<div class=""col-md-2"">
                                <label class=""form-label fw-bold"">Bina / Kapı No</label>
                                <input asp-for=""Address"" id=""fullAddress"" class=""form-control rounded-3"" placeholder=""Örn: 16/A"" required />
                            </div>
                            <div class=""col-md-10"">
                                <label class=""form-label fw-bold"">Harita Konumu</label>
                                <div class=""input-group"">
                                    <input type=""text"" asp-for=""Latitude"" id=""latInput"" class=""form-control bg-light"" placeholder=""Enlem"" readonly />
                                    <input type=""text"" asp-for=""Longitude"" id=""lngInput"" class=""form-control bg-light"" placeholder=""Boylam"" readonly />
                                    <button class=""btn btn-outline-secondary"" type=""button"" onclick=""openMapModal()""><i class=""ph ph-map-pin""></i> Haritadan Seç</button>
                                </div>
                            </div>";

        // Find the "İl, İlçe, Mahalle, Sokak" block
        string illerBlock = @"                            <div class=""col-md-3"">
                                <label class=""form-label fw-bold"">İl</label>
                                <select asp-for=""CityId"" class=""form-select rounded-3"" asp-items=""ViewBag.Cities"" required><option value="""">Seçiniz</option></select>
                            </div>
                            <div class=""col-md-3"">
                                <label class=""form-label fw-bold"">İlçe</label>
                                <select asp-for=""DistrictId"" class=""form-select rounded-3"" asp-items=""ViewBag.Districts"" required><option value="""">Seçiniz</option></select>
                            </div>
                            <div class=""col-md-3"">
                                <label class=""form-label fw-bold"">Mahalle</label>
                                <select asp-for=""NeighborhoodId"" class=""form-select rounded-3"" asp-items=""ViewBag.Neighborhoods"" required><option value="""">Seçiniz</option></select>
                            </div>
                            <div class=""col-md-3"">
                                <label class=""form-label fw-bold"">Sokak</label>
                                <select asp-for=""StreetId"" class=""form-select rounded-3"" asp-items=""ViewBag.Streets"" required><option value="""">Seçiniz</option></select>
                            </div>";

        // Remove kapino block
        if (view.Contains(kapinoBlock) && view.Contains(illerBlock))
        {
            // Remove kapinoBlock from its current position
            view = view.Replace(kapinoBlock, "");
            
            // Insert kapinoBlock exactly after illerBlock
            view = view.Replace(illerBlock, illerBlock + "\n" + kapinoBlock);

            File.WriteAllText(path, view, new UTF8Encoding(true));
            Console.WriteLine("Swapped the blocks.");
        }
        else
        {
            Console.WriteLine("Could not find blocks to swap.");
        }
    }
}
