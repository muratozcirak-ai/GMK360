import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

# I will add EndDate next to StartDate
new_dates = '''<div class="row mb-3">
                        <div class="col-md-4">
                            <label asp-for="StartDate" class="form-label fw-bold">Başlama Tarihi</label>
                            <input asp-for="StartDate" type="date" class="form-control" value="@DateTime.Now.ToString("yyyy-MM-dd")" />
                        </div>
                        <div class="col-md-4">
                            <label asp-for="EndDate" class="form-label fw-bold">Bitiş / Teslim Tarihi</label>
                            <input asp-for="EndDate" type="date" class="form-control" />
                            <small class="text-muted">Eski proje ise teslim yılını seçin.</small>
                        </div>
                        <div class="col-md-4">
                            <label asp-for="Status" class="form-label fw-bold">Proje Durumu</label>
                            <select asp-for="Status" class="form-select">
                                <option value="1">Projelendirme / Teklif</option>
                                <option value="2" selected>Aktif Şantiye</option>
                                <option value="3">Tamamlandı (Geçmiş Proje)</option>
                                <option value="4">Satışta / Topraktan</option>
                            </select>
                        </div>
                    </div>'''

# Replace the old row
import re
pattern = re.compile(r'<div class="row mb-3">.*?<hr class="my-4" />', re.DOTALL)
content = pattern.sub(new_dates + '\r\n\r\n                    <hr class="my-4" />', content)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
