import re

filepath = r'GMK360.Web\Views\Admin\B2bList.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace old change events with the new ones that call .trigger('change') for select2
old_js = """
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

new_js = """
        // Şehir seçildiğinde İlçeleri Getir
        $('#modalCity').on('change', function() {
            var cityId = $(this).val();
            var districtSelect = $('#modalDistrict');
            var neighborhoodSelect = $('#modalNeighborhood');
            
            districtSelect.empty().append('<option value="">İlçe Seçin</option>').trigger('change');
            neighborhoodSelect.empty().append('<option value="">Mahalle Seçin</option>').trigger('change');
            
            if(cityId) {
                $.get('/api/LocationApi/districts/' + cityId, function(data) {
                    $.each(data, function(i, item) {
                        districtSelect.append($('<option>', {
                            value: item.id,
                            text: item.name
                        }));
                    });
                    districtSelect.trigger('change'); // VERY IMPORTANT FOR SELECT2
                }).fail(function() {
                    console.error("İlçeler getirilemedi. API Hatası.");
                    alert("İlçeler getirilirken bir hata oluştu. Lütfen sayfayı yenileyin.");
                });
            }
        });

        // İlçe seçildiğinde Mahalleleri Getir
        $('#modalDistrict').on('change', function() {
            var districtId = $(this).val();
            var neighborhoodSelect = $('#modalNeighborhood');
            
            neighborhoodSelect.empty().append('<option value="">Mahalle Seçin</option>').trigger('change');
            
            if(districtId) {
                $.get('/api/LocationApi/neighborhoods/' + districtId, function(data) {
                    $.each(data, function(i, item) {
                        neighborhoodSelect.append($('<option>', {
                            value: item.id,
                            text: item.name
                        }));
                    });
                    neighborhoodSelect.trigger('change'); // VERY IMPORTANT FOR SELECT2
                }).fail(function() {
                    console.error("Mahalleler getirilemedi. API Hatası.");
                });
            }
        });
"""

# Be careful about encoding or slight formatting mismatches. I'll just use regex to replace everything from "Şehir seçildiğinde" to the end of Mahalleleri Getir block.
content = re.sub(r"// Şehir seçildiğinde İlçeleri Getir.*?\}\);\s*\}\);\s*", new_js, content, flags=re.DOTALL)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
