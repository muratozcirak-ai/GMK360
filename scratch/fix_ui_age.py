import re

filepath = r'GMK360.Web\Views\ConstructionProject\Create.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Add to JavaScript repeater template for ExistingBlocks
pattern1 = r'(<div class="col-6 col-lg-2">\s*<label class="form-label small fw-bold">Kat Sayısı</label>\s*<input type="number" name="ExistingBlocks\[\$\{i\}\]\.TotalFloors" class="form-control" min="1" required />\s*</div>)'
replacement1 = r"""\1
                                  <div class="col-6 col-lg-2">
                                      <label class="form-label small fw-bold">Bina Yaşı (Yıl)</label>
                                      <input type="number" name="ExistingBlocks[${i}].BuildingAge" class="form-control" placeholder="Örn: 25" />
                                  </div>"""
content = re.sub(pattern1, replacement1, content)

# Check if there is a server-side @for loop for ExistingBlocks
# Search for name="ExistingBlocks[@i]
pattern2 = r'(<div class="col-6 col-lg-2">\s*<label class="form-label small fw-bold">Kat Sayısı</label>\s*<input type="number" name="ExistingBlocks\[@i\]\.TotalFloors" class="form-control" value="@Model\.ExistingBlocks\[i\]\.TotalFloors" min="1" required />\s*</div>)'
replacement2 = r"""\1
                                                      <div class="col-6 col-lg-2">
                                                          <label class="form-label small fw-bold">Bina Yaşı (Yıl)</label>
                                                          <input type="number" name="ExistingBlocks[@i].BuildingAge" class="form-control" value="@Model.ExistingBlocks[i].BuildingAge" placeholder="Örn: 25" />
                                                      </div>"""
content = re.sub(pattern2, replacement2, content)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Create.cshtml repeater updated.")
