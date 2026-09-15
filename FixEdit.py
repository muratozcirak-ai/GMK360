import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Edit.cshtml'
with codecs.open(filepath, 'r', 'utf-8') as f:
    content = f.read()

status_html = '''
                    <div class="col-md-12">
                        <label asp-for="Status" class="form-label fw-bold">Proje Durumu</label>
                        <select asp-for="Status" class="form-select rounded-3">
                            <option value="1">Projelendirme / Teklif Aşamasında</option>
                            <option value="2">Aktif Şantiye (Yapım Aşamasında)</option>
                            <option value="4">Satışta (Topraktan)</option>
                            <option value="3">Tamamlandı / Teslim Edildi</option>
                        </select>
                    </div>
'''

content = content.replace('                    <div class="col-md-12">', status_html + '                    <div class="col-md-12">', 1)

with codecs.open(filepath, 'w', 'utf-8') as f:
    f.write(content)
