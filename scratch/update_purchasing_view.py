import re

filepath = r'GMK360.Web\Views\B2BPurchasing\Index.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Add project dropdown before Title
old_html = """                      <div class="mb-3">
                          <label class="form-label fw-bold">Alım Başlığı (Opsiyonel)</label>"""

new_html = """                      <div class="mb-3">
                          <label class="form-label fw-bold text-primary">Hangi Proje / Şantiye İçin?</label>
                          <select name="projectId" class="form-select border-primary" required>
                              <option value="">-- Proje Seçiniz --</option>
                              @if (ViewBag.Projects != null)
                              {
                                  foreach(var proj in (IEnumerable<GMK360.Core.Entities.Construction.ConstructionProject>)ViewBag.Projects)
                                  {
                                      <option value="@proj.Id">@proj.Name</option>
                                  }
                              }
                          </select>
                      </div>
                      <div class="mb-3">
                          <label class="form-label fw-bold">Alım Başlığı (Opsiyonel)</label>"""

# Fix encoding replacements
old_html_search = """                      <div class="mb-3">
                          <label class="form-label fw-bold">Alm Bal (Opsiyonel)</label>"""

new_html_encoded = """                      <div class="mb-3">
                          <label class="form-label fw-bold text-primary">Hangi Proje / antiye in?</label>
                          <select name="projectId" class="form-select border-primary" required>
                              <option value="">-- Ltfen Proje Seiniz --</option>
                              @if (ViewBag.Projects != null)
                              {
                                  foreach(var proj in (IEnumerable<GMK360.Core.Entities.Construction.ConstructionProject>)ViewBag.Projects)
                                  {
                                      <option value="@proj.Id">@proj.Name</option>
                                  }
                              }
                          </select>
                      </div>
                      <div class="mb-3">
                          <label class="form-label fw-bold">Alm Bal (Opsiyonel)</label>"""

content = content.replace(old_html_search, new_html_encoded)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("View updated.")
