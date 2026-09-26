import re

filepath = r'GMK360.Web\Views\Admin\B2bList.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

js_addition = """
        // Şehir seçildiğinde İlçeleri Getir
        $('#modalCity').change(function() {
            var cityId = $(this).val();
            var districtSelect = $('#modalDistrict');
            var neighborhoodSelect = $('#modalNeighborhood');
            
            districtSelect.empty().append('<option value="">İlçe Seçin</option>');
            neighborhoodSelect.empty().append('<option value="">Mahalle Seçin</option>');
            
            if(cityId) {
                $.get('/api/LocationApi/districts/' + cityId, function(data) {
                    $.each(data, function(i, item) {
                        districtSelect.append($('<option>', {
                            value: item.id,
                            text: item.name
                        }));
                    });
                });
            }
        });

        // İlçe seçildiğinde Mahalleleri Getir
        $('#modalDistrict').change(function() {
            var districtId = $(this).val();
            var neighborhoodSelect = $('#modalNeighborhood');
            
            neighborhoodSelect.empty().append('<option value="">Mahalle Seçin</option>');
            
            if(districtId) {
                $.get('/api/LocationApi/neighborhoods/' + districtId, function(data) {
                    $.each(data, function(i, item) {
                        neighborhoodSelect.append($('<option>', {
                            value: item.id,
                            text: item.name
                        }));
                    });
                });
            }
        });
"""

if "$.get('/api/LocationApi/districts/'" not in content:
    content = content.replace("// Eğer Kurumsal/Tedarikçi sekmesindeysek", js_addition + "\n        // Eğer Kurumsal/Tedarikçi sekmesindeysek")

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
