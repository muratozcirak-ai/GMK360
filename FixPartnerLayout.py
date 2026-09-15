import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_PartnerLayout.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

# Instead of blindly replacing, let's parse where the menu links are and append them.
menu_insert = '''
                <!-- Taşeron / Tedarikçi Linkleri -->
                <a href="/PartnerPortal/Workers" class="nav-link text-white py-2 mb-1 rounded px-3" style="transition: all 0.2s;">
                    <i class="bi bi-people me-2 opacity-75"></i> Ekiplerim (Personel)
                </a>
                <a href="/PartnerPortal/Timesheets" class="nav-link text-white py-2 mb-1 rounded px-3" style="transition: all 0.2s;">
                    <i class="bi bi-calendar-check me-2 opacity-75"></i> Kör Puantaj (Yoklama)
                </a>
                <a href="/PartnerPortal/Contracts" class="nav-link text-white py-2 mb-1 rounded px-3" style="transition: all 0.2s;">
                    <i class="bi bi-file-earmark-text me-2 opacity-75"></i> Sözleşme ve Hakedişler
                </a>
'''

# We know we have a list of nav links, maybe "Bize Ulaşın" or something
if "Ekiplerim" not in content:
    content = content.replace('<!-- Ekstra menüler buraya eklenebilir -->', menu_insert + '\n<!-- Ekstra menüler buraya eklenebilir -->')
    content = content.replace('</ul>\r\n\r\n                <div', menu_insert + '\n</ul>\r\n\r\n                <div') # fallback

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
