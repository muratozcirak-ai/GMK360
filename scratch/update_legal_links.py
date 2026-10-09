import re

with open(r'GMK360.Web\Views\Shared\_Layout.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Update Legal Links
old_kvkk = '''<li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">KVKK Aydınlatma Metni</a></li>'''
new_kvkk = '''<li><a href="/Home/Legal?doc=kvkk" class="text-white-50 text-decoration-none hover-white transition-300">KVKK Aydınlatma Metni</a></li>'''
content = content.replace(old_kvkk, new_kvkk)

old_terms = '''<li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">Kullanım Koşulları</a></li>'''
new_terms = '''<li><a href="/Home/Legal?doc=terms" class="text-white-50 text-decoration-none hover-white transition-300">Kullanım Koşulları</a></li>'''
content = content.replace(old_terms, new_terms)

old_privacy = '''<li><a href="#" class="text-white-50 text-decoration-none hover-white transition-300">Gizlilik Politikası</a></li>'''
new_privacy = '''<li><a href="/Home/Legal?doc=privacy" class="text-white-50 text-decoration-none hover-white transition-300">Gizlilik Politikası</a></li>'''
content = content.replace(old_privacy, new_privacy)

with open(r'GMK360.Web\Views\Shared\_Layout.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
