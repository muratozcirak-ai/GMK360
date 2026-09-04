using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string view = File.ReadAllText(path, Encoding.UTF8);

        string oldAddressBlock = @"<div class=""col-md-12"">
                                <label class=""form-label fw-bold"">Açık Adres (Tam lokasyon)</label>
                                <textarea asp-for=""Address"" id=""fullAddress"" class=""form-control rounded-3"" rows=""2"" placeholder=""Örn: Cumhuriyet Mah. Vatan Cad. No:16/A"" required></textarea>
                            </div>";

        string newAddressBlock = @"<div class=""col-md-2"">
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

        if (view.Contains(oldAddressBlock))
        {
            view = view.Replace(oldAddressBlock, newAddressBlock);
        }

        string mapModal = @"
<!-- Map Modal -->
<div class=""modal fade"" id=""mapModal"" tabindex=""-1"">
    <div class=""modal-dialog modal-lg modal-dialog-centered"">
        <div class=""modal-content"">
            <div class=""modal-header"">
                <h5 class=""modal-title"">Haritadan Konum Seç</h5>
                <button type=""button"" class=""btn-close"" data-bs-dismiss=""modal""></button>
            </div>
            <div class=""modal-body p-0"">
                <div id=""projectMap"" style=""height: 400px; width: 100%;""></div>
            </div>
            <div class=""modal-footer"">
                <button type=""button"" class=""btn btn-secondary"" data-bs-dismiss=""modal"">İptal</button>
                <button type=""button"" class=""btn btn-primary"" onclick=""saveMapLocation()"">Konumu Onayla</button>
            </div>
        </div>
    </div>
</div>
";
        
        string mapScripts = @"
        let map, marker;
        function openMapModal() {
            var mapModal = new bootstrap.Modal(document.getElementById('mapModal'));
            mapModal.show();
            
            setTimeout(() => {
                if(!map) {
                    map = L.map('projectMap').setView([41.0082, 28.9784], 13);
                    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                        attribution: '© OpenStreetMap contributors'
                    }).addTo(map);
                    
                    marker = L.marker([41.0082, 28.9784], {draggable: true}).addTo(map);
                    
                    map.on('click', function(e) {
                        marker.setLatLng(e.latlng);
                    });
                } else {
                    map.invalidateSize();
                }
            }, 300);
        }
        
        function saveMapLocation() {
            var pos = marker.getLatLng();
            document.getElementById('latInput').value = pos.lat.toFixed(6);
            document.getElementById('lngInput').value = pos.lng.toFixed(6);
            bootstrap.Modal.getInstance(document.getElementById('mapModal')).hide();
        }
";

        if (!view.Contains("id=\"mapModal\""))
        {
            view = view.Replace("</form>", "</form>\n" + mapModal);
            view = view.Replace("function nextStep", mapScripts + "\n        function nextStep");
        }

        File.WriteAllText(path, view, new UTF8Encoding(true));
        Console.WriteLine("Done.");
    }
}
