import io

filepath = r'GMK360.Web\Views\PhaseZero\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Update the badges for Main Docs
content = content.replace('@if (doc.Status == "Bekliyor") { <span class="status-badge badge-waiting">Bekliyor</span> }', '@if (doc.Status == "Bekliyor") { <span class="status-badge badge-waiting">Bekliyor</span> }')
content = content.replace('else if (doc.Status == "İşlemde") { <span class="status-badge badge-process">İşlemde</span> }', 'else if (doc.Status == "İşlemde") { <span class="status-badge badge-process">Başvuru Yapıldı</span> }')
content = content.replace('else if (doc.Status == "Sorunlu") { <span class="status-badge badge-danger">Sorunlu</span> }', 'else if (doc.Status == "Sorunlu") { <span class="status-badge badge-danger">Sorun Çıktı</span> }')
content = content.replace('else if (doc.Status == "Alındı") { <span class="status-badge badge-success">Alındı</span> }', 'else if (doc.Status == "Alındı") { <span class="status-badge badge-success">Tamamlandı</span> }')

# 1. Update the badges for Child Docs
content = content.replace('@if (childDoc.Status == "Bekliyor") { <span class="status-badge badge-waiting">Bekliyor</span> }', '@if (childDoc.Status == "Bekliyor") { <span class="status-badge badge-waiting">Bekliyor</span> }')
content = content.replace('else if (childDoc.Status == "İşlemde") { <span class="status-badge badge-process">İşlemde</span> }', 'else if (childDoc.Status == "İşlemde") { <span class="status-badge badge-process">Başvuru Yapıldı</span> }')
content = content.replace('else if (childDoc.Status == "Sorunlu") { <span class="status-badge badge-danger">Sorunlu</span> }', 'else if (childDoc.Status == "Sorunlu") { <span class="status-badge badge-danger">Sorun Çıktı</span> }')
content = content.replace('else if (childDoc.Status == "Alındı") { <span class="status-badge badge-success">Alındı</span> }', 'else if (childDoc.Status == "Alındı") { <span class="status-badge badge-success">Tamamlandı</span> }')


with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated PhaseZero/Index.cshtml badges")
