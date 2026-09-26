import re

filepath = r'GMK360.Web\Views\Admin\B2bList.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# I will append the JS inside the @section Scripts block
js_code = """
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
        
        // Eğer Kurumsal/Tedarikçi sekmesindeysek varsayılan olarak Şirket'i seç
        if('@listType' == 'firma' || '@listType' == 'tedarikci') {
            $('#legalCorporate').prop('checked', true).trigger('change');
        }
"""

if js_code not in content:
    content = content.replace("theme: \"classic\",\n                width: 'resolve'\n            });", "theme: \"classic\",\n                width: 'resolve'\n            });\n" + js_code)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
