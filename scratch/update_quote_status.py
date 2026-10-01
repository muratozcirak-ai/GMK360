import io

filepath = r'GMK360.Web\Controllers\PhaseZeroController.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('doc.Status = "İşlemde"; // Satınalma sürecine girdi', 'doc.Status = "Fiyat Araştırılıyor"; // Satınalma sürecine girdi')

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated PhaseZeroController.cs status logic.")
