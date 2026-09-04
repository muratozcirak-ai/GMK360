using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string view = File.ReadAllText(path, Encoding.UTF8);

        string oldScript = @"                    fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(query)}&limit=1`)
                        .then(res => res.json())
                        .then(data => {
                            if(data && data.length > 0) {
                                var lat = parseFloat(data[0].lat);
                                var lon = parseFloat(data[0].lon);
                                map.setView([lat, lon], 16);
                                marker.setLatLng([lat, lon]);
                            }
                        });";

        string newScript = @"                    var tryGeocode = function(searchQuery, zoomLvl) {
                        fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(searchQuery)}&limit=1`)
                        .then(res => res.json())
                        .then(data => {
                            if(data && data.length > 0) {
                                var lat = parseFloat(data[0].lat);
                                var lon = parseFloat(data[0].lon);
                                map.setView([lat, lon], zoomLvl);
                                marker.setLatLng([lat, lon]);
                            } else {
                                // Bulamazsa bir üste geç
                                if (searchQuery.includes(streetName) && streetName) {
                                    tryGeocode(`${neighName}, ${distName}, ${cityName}, Turkey`, 15);
                                } else if (searchQuery.includes(neighName) && neighName) {
                                    tryGeocode(`${distName}, ${cityName}, Turkey`, 13);
                                }
                            }
                        });
                    };
                    
                    tryGeocode(query, 16);";

        if (view.Contains("fetch(`https://nominatim.openstreetmap.org/search"))
        {
            view = view.Replace(oldScript, newScript);
            File.WriteAllText(path, view, new UTF8Encoding(true));
            Console.WriteLine("Map fallback geocoding added.");
        }
    }
}
