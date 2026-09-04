using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string view = File.ReadAllText(path, Encoding.UTF8);

        string cascadingJs = @"
        $(document).ready(function() {
            // Sehir degisince ilceyi doldur
            $('#CityId').change(function() {
                var cityId = $(this).val();
                $('#DistrictId').empty().append('<option value="""">Yükleniyor...</option>');
                $('#NeighborhoodId').empty().append('<option value="""">Seçiniz</option>');
                $('#StreetId').empty().append('<option value="""">Seçiniz</option>');
                
                if (cityId) {
                    $.get('/api/Location/Districts/' + cityId, function(data) {
                        $('#DistrictId').empty().append('<option value="""">Seçiniz</option>');
                        if(data && data.length > 0) {
                            data.forEach(function(d) {
                                $('#DistrictId').append('<option value=""' + d.id + '"">' + d.name + '</option>');
                            });
                        }
                    });
                } else {
                    $('#DistrictId').empty().append('<option value="""">Seçiniz</option>');
                }
            });

            // Ilce degisince mahalleyi doldur
            $('#DistrictId').change(function() {
                var districtId = $(this).val();
                $('#NeighborhoodId').empty().append('<option value="""">Yükleniyor...</option>');
                $('#StreetId').empty().append('<option value="""">Seçiniz</option>');
                
                if (districtId) {
                    $.get('/api/Location/Neighborhoods/' + districtId, function(data) {
                        $('#NeighborhoodId').empty().append('<option value="""">Seçiniz</option>');
                        if(data && data.length > 0) {
                            data.forEach(function(n) {
                                $('#NeighborhoodId').append('<option value=""' + n.id + '"">' + n.name + '</option>');
                            });
                        }
                    });
                } else {
                    $('#NeighborhoodId').empty().append('<option value="""">Seçiniz</option>');
                }
            });

            // Mahalle degisince sokagi doldur
            $('#NeighborhoodId').change(function() {
                var neighborhoodId = $(this).val();
                $('#StreetId').empty().append('<option value="""">Yükleniyor...</option>');
                
                if (neighborhoodId) {
                    $.get('/api/Location/Streets/' + neighborhoodId, function(data) {
                        $('#StreetId').empty().append('<option value="""">Seçiniz</option>');
                        if(data && data.length > 0) {
                            data.forEach(function(s) {
                                $('#StreetId').append('<option value=""' + s.id + '"">' + s.name + '</option>');
                            });
                        }
                    });
                } else {
                    $('#StreetId').empty().append('<option value="""">Seçiniz</option>');
                }
            });
        });
        ";

        if (!view.Contains("$('#CityId').change("))
        {
            view = view.Replace("function toggleParent(sel)", cascadingJs + "\n        function toggleParent(sel)");
            File.WriteAllText(path, view, new UTF8Encoding(true));
            Console.WriteLine("Cascading JS restored.");
        }
        else
        {
            Console.WriteLine("Cascading JS already present.");
        }
    }
}
