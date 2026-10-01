import io
import re

filepath = r'GMK360.Web\Views\Dashboard\SiteYonetimi.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace("ProjectStatus.Planlama_Faz0", "ProjectStatus.On_Gorusme_Talep")

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Fixed ProjectStatus reference")
