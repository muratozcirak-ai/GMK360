import re

filepath = r'GMK360.Web\Views\Admin\B2bList.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

scripts_start = content.find("@section Scripts {")
if scripts_start != -1:
    new_scripts = """@section Scripts {
    <link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>
    <script>
        $(document).ready(function() {
            $('.select2-search').select2({
                theme: "classic",
                width: 'resolve',
                dropdownParent: $('#addModal')
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
                
                // Clear and disable while loading
                districtSelect.empty().append('<option value="">İlçe Seçin</option>').prop('disabled', true).trigger('change');
                neighborhoodSelect.empty().append('<option value="">Mahalle Seçin</option>').prop('disabled', true).trigger('change');
                
                if(cityId) {
                    $.ajax({
                        url: '/AdminLocation/GetDistricts',
                        type: 'GET',
                        data: { cityId: cityId },
                        dataType: 'json',
                        success: function(data) {
                            districtSelect.empty().append('<option value="">İlçe Seçin</option>');
                            
                            if(!data || data.length === 0) {
                                alert("Seçilen ile ait ilçe bulunamadı.");
                            } else {
                                $.each(data, function(i, item) {
                                    districtSelect.append($('<option>', {
                                        value: item.id,
                                        text: item.name
                                    }));
                                });
                            }
                            districtSelect.prop('disabled', false).trigger('change');
                        },
                        error: function(xhr, status, error) {
                            alert("İlçeler getirilirken hata: " + error);
                            districtSelect.prop('disabled', false);
                        }
                    });
                } else {
                    districtSelect.prop('disabled', false).trigger('change');
                    neighborhoodSelect.prop('disabled', false).trigger('change');
                }
            });

            // İlçe seçildiğinde Mahalleleri Getir
            $('#modalDistrict').on('change', function() {
                var districtId = $(this).val();
                var neighborhoodSelect = $('#modalNeighborhood');
                
                neighborhoodSelect.empty().append('<option value="">Mahalle Seçin</option>').prop('disabled', true).trigger('change');
                
                if(districtId) {
                    $.ajax({
                        url: '/AdminLocation/GetNeighborhoods',
                        type: 'GET',
                        data: { districtId: districtId },
                        dataType: 'json',
                        success: function(data) {
                            neighborhoodSelect.empty().append('<option value="">Mahalle Seçin</option>');
                            $.each(data, function(i, item) {
                                neighborhoodSelect.append($('<option>', {
                                    value: item.id,
                                    text: item.name
                                }));
                            });
                            neighborhoodSelect.prop('disabled', false).trigger('change');
                        },
                        error: function(xhr, status, error) {
                            alert("Mahalleler getirilirken hata: " + error);
                            neighborhoodSelect.prop('disabled', false);
                        }
                    });
                } else {
                    neighborhoodSelect.prop('disabled', false).trigger('change');
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
