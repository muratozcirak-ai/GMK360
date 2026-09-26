import re

filepath = r'GMK360.Web\Views\Admin\B2bList.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# I will replace the ENTIRE Scripts section to ensure it's clean and syntactically correct.
scripts_start = content.find("@section Scripts {")
if scripts_start != -1:
    new_scripts = """@section Scripts {
    <link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>
    <script>
        $(document).ready(function() {
            $('.select2-search').select2({
                theme: "classic",
                width: 'resolve'
            });

            // Hukuki Statü Değişimi
            $('.legal-status-radio').change(function() {
                if(this.value == '1') {
                    $('#individualFields').show();
                    $('#corporateFields').hide();
                } else {
                    $('#individualFields').hide();
                    $('#corporateFields').show();
                }
            });
            
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
                        districtSelect.trigger('change');
                    }).fail(function() {
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
                        neighborhoodSelect.trigger('change');
                    }).fail(function() {
                        console.error("Mahalleler getirilemedi. API Hatası.");
                    });
                }
            });

            // Eğer Kurumsal/Tedarikçi sekmesindeysek varsayılan olarak Şirket'i seç
            if('@ViewBag.ListType' == 'firma' || '@ViewBag.ListType' == 'tedarikci') {
                $('#legalCorporate').prop('checked', true).trigger('change');
            }
        });
    </script>
}"""
    content = content[:scripts_start] + new_scripts
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
