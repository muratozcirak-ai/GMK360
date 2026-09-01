import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\CustomerDashboard\Index.cshtml"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

card_html = """
        @if (User.IsInRole("InsaatFirmasi") || User.IsInRole("Admin"))
        {
            <!-- İNŞAAT VE ŞANTİYE YÖNETİMİ -->
            <div class="col-md-4">
                <a href="/ConstructionProject/Index" class="text-decoration-none">
                    <div class="card h-100 shadow-sm border-0 text-white text-center p-4" style="background-color: #f97316; transition: transform 0.2s; border-radius: 1rem;">
                        <i class="ph ph-crane" style="font-size: 4rem; opacity: 0.9;"></i>
                        <h4 class="fw-bold mt-3 text-white">İnşaat Projesi Yönetimi</h4>
                        <p class="small text-white-50 mt-2">Şantiyeleriniz, taşeronlar ve yevmiyeli işçi puantajları.</p>
                    </div>
                </a>
            </div>
        }
"""
if "ConstructionProject/Index" not in content:
    content = content.replace('<!-- G', card_html + '<!-- G', 1)
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
    print("Added construction card.")
else:
    print("Already added.")
