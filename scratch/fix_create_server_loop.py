import re

filepath = r'GMK360.Web\Views\ConstructionProject\Create.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Fix the server-side loop for ExistingBlocks to include Building Age
pattern_server = r'(<div class="col-6 col-lg-2">\s*<label class="form-label small fw-bold">Kat Sayısı</label>\s*<input type="number" name="ExistingBlocks\[@i\]\.TotalFloors" class="form-control" min="1" value="@b\.TotalFloors" required />\s*</div>)'
replacement_server = r"""\1
                                                      <div class="col-6 col-lg-2">
                                                          <label class="form-label small fw-bold">Bina Yaşı (Yıl)</label>
                                                          <input type="number" name="ExistingBlocks[@i].BuildingAge" class="form-control" value="@b.BuildingAge" placeholder="Örn: 25" />
                                                      </div>"""
content = re.sub(pattern_server, replacement_server, content)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Create.cshtml server-side loop updated.")
