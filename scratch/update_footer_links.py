import re

with open(r'GMK360.Web\Views\Shared\_Layout.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# 1. Update Social Media
old_social = '''<div class="d-flex gap-3">
                        <a href="#" class="text-white-50 text-decoration-none hover-orange transition-300"><i class="bi bi-linkedin fs-5"></i></a>
                        <a href="#" class="text-white-50 text-decoration-none hover-orange transition-300"><i class="bi bi-instagram fs-5"></i></a>
                        <a href="#" class="text-white-50 text-decoration-none hover-orange transition-300"><i class="bi bi-youtube fs-5"></i></a>
                    </div>'''
new_social = '''<div class="d-flex gap-3 position-relative">
                        <a href="#" class="text-white-50 text-decoration-none hover-orange transition-300" title="Sosyal Medya (Çok Yakında)"><i class="bi bi-linkedin fs-5"></i></a>
                        <a href="#" class="text-white-50 text-decoration-none hover-orange transition-300" title="Sosyal Medya (Çok Yakında)"><i class="bi bi-instagram fs-5"></i></a>
                        <a href="#" class="text-white-50 text-decoration-none hover-orange transition-300" title="Sosyal Medya (Çok Yakında)"><i class="bi bi-youtube fs-5"></i></a>
                        <div class="position-absolute" style="top: -20px; left: 10px;">
                            <span class="badge bg-orange text-white" style="font-size: 0.6rem;">Çok Yakında</span>
                        </div>
                    </div>'''
content = content.replace(old_social, new_social)

# 2. Update Çözümler list to contain 8 modules
old_cozum = '''<ul class="list-unstyled d-flex flex-column gap-3 small">
                        <li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">Şantiye & İnşaat ERP</a></li>
                        <li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">Bina & Site Yönetimi</a></li>
                        <li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">Tedarikçi Pazar Yeri</a></li>
                        <li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">Emlak & Acente Ağı</a></li>
                        <li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">Usta & Hizmet Ekosistemi</a></li>
                    </ul>'''
new_cozum = '''<ul class="list-unstyled d-flex flex-column gap-2 small">
                        <li><a href="/Modules/Insaat" class="text-white-50 text-decoration-none hover-white transition-300">Şantiye & İnşaat ERP</a></li>
                        <li><a href="/Modules/Bina" class="text-white-50 text-decoration-none hover-white transition-300">Bina, Site & AVM</a></li>
                        <li><a href="/Modules/Mulk" class="text-white-50 text-decoration-none hover-white transition-300">Mülk & Kiracı Takibi</a></li>
                        <li><a href="/Modules/Emlak" class="text-white-50 text-decoration-none hover-white transition-300">Emlak Ofisleri Ağı</a></li>
                        <li><a href="/Modules/KisaDonem" class="text-white-50 text-decoration-none hover-white transition-300">Günlük Kiralık (Airbnb)</a></li>
                        <li><a href="/Modules/Tedarik" class="text-white-50 text-decoration-none hover-white transition-300">Tedarikçi Pazar Yeri</a></li>
                        <li><a href="/Modules/Usta" class="text-white-50 text-decoration-none hover-white transition-300">Usta & Tadilat Ağı</a></li>
                        <li><a href="/Modules/Vip" class="text-white-50 text-decoration-none hover-white transition-300 text-warning">VIP Varlık & Portföy</a></li>
                    </ul>'''
content = content.replace(old_cozum, new_cozum)

# 3. Update Akıllı Asistan to point to /Home/Faq
old_asistan = '''<button class="btn btn-orange btn-sm text-start fw-bold shadow-sm mt-2"><i class="bi bi-robot me-2"></i> Akıllı Asistan'a Sor</button>'''
new_asistan = '''<a href="/Home/Faq" class="btn btn-orange btn-sm text-start fw-bold shadow-sm mt-2 text-white text-decoration-none"><i class="bi bi-robot me-2"></i> Akıllı Asistan'a Sor</a>'''
content = content.replace(old_asistan, new_asistan)

# Also update SSS to point to /Home/Faq
old_sss = '''<li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">Sıkça Sorulan Sorular (SSS)</a></li>'''
new_sss = '''<li><a href="/Home/Faq" class="text-white-50 text-decoration-none hover-white transition-300">Sıkça Sorulan Sorular (SSS)</a></li>'''
content = content.replace(old_sss, new_sss)

with open(r'GMK360.Web\Views\Shared\_Layout.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
