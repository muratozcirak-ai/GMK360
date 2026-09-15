import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Details.cshtml'
with codecs.open(filepath, 'r', 'utf-8') as f:
    content = f.read()

# 1. Remove Şantiye Bilgileri block
pattern_remove = re.compile(r'<div class="card shadow-sm border-0 rounded-4 mb-4">\s*<div class="row g-0">.*?</div>\s*</div>\s*</div>\s*<!-- BARIYER \(FAZ 0\) UYARISI YERINE GERCEK TABLO -->', re.DOTALL)
content = pattern_remove.sub('<!-- BARIYER (FAZ 0) UYARISI YERINE GERCEK TABLO -->', content)

# 2. Add map button to Proje Özeti
map_html = '''                  <p>@Model.Description</p>
                  
                  @if(Model.Latitude != null && Model.Longitude != null)
                  {
                      <div class="mb-3">
                          <a href="https://www.google.com/maps?q=@Model.Latitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),@Model.Longitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)" target="_blank" class="btn btn-sm btn-outline-danger rounded-pill shadow-sm">
                              <i class="bi bi-geo-alt-fill me-1"></i> Lojistik ve Harita Konumu (Google Maps)
                          </a>
                      </div>
                  }'''

content = content.replace('                  <p>@Model.Description</p>', map_html)

with codecs.open(filepath, 'w', 'utf-8') as f:
    f.write(content)
