using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string view = File.ReadAllText(path, Encoding.UTF8);

        string oldScript = @"        let map, marker;
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
        }";

        string newScript = @"        let map, marker;
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

                // Harita açıldığında seçili adrese odaklan (Geocoding)
                var cityName = $('#CityId option:selected').text();
                var distName = $('#DistrictId option:selected').text();
                var neighName = $('#NeighborhoodId option:selected').text();
                var streetName = $('#StreetId option:selected').text();
                
                if (cityName && cityName !== 'Seçiniz') {
                    // Mümkün olan en detaylı adresi oluştur
                    var query = '';
                    if (neighName && neighName !== 'Seçiniz' && neighName !== 'Yükleniyor...') {
                        if (streetName && streetName !== 'Seçiniz' && streetName !== 'Yükleniyor...') {
                            query = `${streetName}, ${neighName}, ${distName}, ${cityName}, Turkey`;
                        } else {
                            // Sokak yoksa mahalleye odaklan (Kullanıcı talebi)
                            query = `${neighName}, ${distName}, ${cityName}, Turkey`;
                        }
                    } else if (distName && distName !== 'Seçiniz') {
                        query = `${distName}, ${cityName}, Turkey`;
                    } else {
                        query = `${cityName}, Turkey`;
                    }

                    fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(query)}&limit=1`)
                        .then(res => res.json())
                        .then(data => {
                            if(data && data.length > 0) {
                                var lat = parseFloat(data[0].lat);
                                var lon = parseFloat(data[0].lon);
                                map.setView([lat, lon], 16);
                                marker.setLatLng([lat, lon]);
                            }
                        });
                }
            }, 300);
        }";

        if (view.Contains("function openMapModal()"))
        {
            view = view.Replace(oldScript, newScript);
            File.WriteAllText(path, view, new UTF8Encoding(true));
            Console.WriteLine("Map geocoding focus updated.");
        }
    }
}
