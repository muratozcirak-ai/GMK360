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
            // Select2'yi Modal ile uyumlu balat
            $('.select2-search').select2({
                theme: "classic",
                width: 'resolve',
                dropdownParent: $('#addModal')
            });

            // Hukuki Stat Deiimi
            $('.legal-status-radio').change(function() {
                if(this.value == '1') {
                    $('#individualFields').show();
                    $('#corporateFields').hide();
                } else {
                    $('#individualFields').hide();
                    $('#corporateFields').show();
                }
            });
            
            // Eer Kurumsal/Tedariki sekmesindeysek varsaylan olarak irket'i se
            if('@ViewBag.ListType' == 'firma' || '@ViewBag.ListType' == 'tedarikci') {
                $('#legalCorporate').prop('checked', true).trigger('change');
            }
            
            // ========================================================
            // B2B KONUM YKLEME ALGORTMASI (Vanilla JS + Fetch)
            // ========================================================
            var citySelect = document.getElementById('modalCity');
            var districtSelect = document.getElementById('modalDistrict');
            var neighborhoodSelect = document.getElementById('modalNeighborhood');
            
            // 1. ehir seildiinde leleri Getir
            $('#modalCity').on('change', function() {
                var cityId = this.value;
                
                // i boalt ve ykleniyor yaz
                districtSelect.innerHTML = '<option value="">Ykleniyor...</option>';
                neighborhoodSelect.innerHTML = '<option value="">nce le Seiniz...</option>';
                $('#modalDistrict').trigger('change.select2');
                $('#modalNeighborhood').trigger('change.select2');
                
                if(!cityId) {
                    districtSelect.innerHTML = '<option value="">le Sein</option>';
                    $('#modalDistrict').trigger('change.select2');
                    return;
                }
                
                // Veritabanndan ek
                fetch('/AdminLocation/GetDistricts?cityId=' + cityId)
                    .then(res => res.json())
                    .then(data => {
                        districtSelect.innerHTML = '<option value="">le Sein</option>';
                        if(data && data.length > 0) {
                            data.forEach(d => {
                                districtSelect.innerHTML += `<option value="${d.id}">${d.name}</option>`;
                            });
                        } else {
                            districtSelect.innerHTML = '<option value="">Bu ile ait ile bulunamad</option>';
                        }
                        // Grseli yenile
                        $('#modalDistrict').trigger('change.select2');
                    })
                    .catch(err => {
                        console.error(err);
                        districtSelect.innerHTML = '<option value="">Balant Hatas</option>';
                        $('#modalDistrict').trigger('change.select2');
                    });
            });

            // 2. le seildiinde Mahalleleri Getir
            $('#modalDistrict').on('change', function() {
                var districtId = this.value;
                
                neighborhoodSelect.innerHTML = '<option value="">Ykleniyor...</option>';
                $('#modalNeighborhood').trigger('change.select2');
                
                if(!districtId) {
                    neighborhoodSelect.innerHTML = '<option value="">Mahalle Sein</option>';
                    $('#modalNeighborhood').trigger('change.select2');
                    return;
                }
                
                fetch('/AdminLocation/GetNeighborhoods?districtId=' + districtId)
                    .then(res => res.json())
                    .then(data => {
                        neighborhoodSelect.innerHTML = '<option value="">Mahalle Sein</option>';
                        if(data && data.length > 0) {
                            data.forEach(n => {
                                neighborhoodSelect.innerHTML += `<option value="${n.id}">${n.name}</option>`;
                            });
                        } else {
                            neighborhoodSelect.innerHTML = '<option value="">Bu ileye ait mahalle bulunamad</option>';
                        }
                        $('#modalNeighborhood').trigger('change.select2');
                    })
                    .catch(err => {
                        console.error(err);
                        neighborhoodSelect.innerHTML = '<option value="">Balant Hatas</option>';
                        $('#modalNeighborhood').trigger('change.select2');
                    });
            });
        });
    </script>
}"""
    content = content[:scripts_start] + new_scripts
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
