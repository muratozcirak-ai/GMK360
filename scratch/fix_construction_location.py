import re

filepath = r'GMK360.Web\Views\ConstructionProject\Create.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

location_script = """
        // ========================================================
        // IL/ILCE/MAHALLE/SOKAK DINAMIK YUKLEME (AJAX)
        // ========================================================
        var cpCity = document.getElementById('CityId');
        var cpDist = document.getElementById('DistrictId');
        var cpNeigh = document.getElementById('NeighborhoodId');
        var cpStreet = document.getElementById('StreetId');

        if(cpCity) {
            cpCity.addEventListener('change', function() {
                var cityId = this.value;
                cpDist.innerHTML = '<option value="">Yükleniyor...</option>';
                if(cpNeigh) cpNeigh.innerHTML = '<option value="">Önce İlçe Seçin</option>';
                if(cpStreet) cpStreet.innerHTML = '<option value="">Önce Mahalle Seçin</option>';

                if(!cityId) {
                    cpDist.innerHTML = '<option value="">İlçe Seçin</option>';
                    return;
                }

                fetch('/api/Location/Districts/' + cityId)
                    .then(res => res.json())
                    .then(data => {
                        cpDist.innerHTML = '<option value="">İlçe Seçin</option>';
                        if(data && data.length > 0) {
                            let currentGroup = '';
                            let html = '';
                            data.forEach(d => {
                                if(d.regionName) {
                                    if(currentGroup !== d.regionName) {
                                        if(currentGroup !== '') html += '</optgroup>';
                                        currentGroup = d.regionName;
                                        html += `<optgroup label="${currentGroup}">`;
                                    }
                                } else {
                                    if(currentGroup !== '') {
                                        html += '</optgroup>';
                                        currentGroup = '';
                                    }
                                }
                                html += `<option value="${d.id}">${d.name}</option>`;
                            });
                            if(currentGroup !== '') html += '</optgroup>';
                            cpDist.innerHTML += html;
                        } else {
                            cpDist.innerHTML = '<option value="">Bulunamadı</option>';
                        }
                    }).catch(err => {
                        console.error(err);
                        cpDist.innerHTML = '<option value="">Hata Oluştu</option>';
                    });
            });
        }

        if(cpDist) {
            cpDist.addEventListener('change', function() {
                var districtId = this.value;
                if(cpNeigh) cpNeigh.innerHTML = '<option value="">Yükleniyor...</option>';
                if(cpStreet) cpStreet.innerHTML = '<option value="">Önce Mahalle Seçin</option>';
                
                if(!districtId) {
                    if(cpNeigh) cpNeigh.innerHTML = '<option value="">Mahalle Seçin</option>';
                    return;
                }

                fetch('/api/Location/Neighborhoods/' + districtId)
                    .then(res => res.json())
                    .then(data => {
                        if(cpNeigh) {
                            cpNeigh.innerHTML = '<option value="">Mahalle Seçin</option>';
                            if(data && data.length > 0) {
                                data.forEach(n => {
                                    cpNeigh.innerHTML += `<option value="${n.id}">${n.name}</option>`;
                                });
                            } else {
                                cpNeigh.innerHTML = '<option value="">Bulunamadı</option>';
                            }
                        }
                    });
            });
        }

        if(cpNeigh && cpStreet) {
            cpNeigh.addEventListener('change', function() {
                var neighId = this.value;
                cpStreet.innerHTML = '<option value="">Yükleniyor...</option>';
                
                if(!neighId) {
                    cpStreet.innerHTML = '<option value="">Sokak Seçin</option>';
                    return;
                }

                fetch('/api/Location/Streets/' + neighId)
                    .then(res => res.json())
                    .then(data => {
                        cpStreet.innerHTML = '<option value="">Sokak Seçin</option>';
                        if(data && data.length > 0) {
                            data.forEach(s => {
                                cpStreet.innerHTML += `<option value="${s.id}">${s.name}</option>`;
                            });
                        } else {
                            cpStreet.innerHTML = '<option value="">Bulunamadı</option>';
                        }
                    });
            });
        }
"""

content = content.replace("function handleStatusChange() {", location_script + "\n\nfunction handleStatusChange() {")

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
