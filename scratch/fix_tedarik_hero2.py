import re
with open(r'GMK360.Web\Views\Modules\Tedarik.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

old_hero = '''<h1 class="display-4 fw-bolder text-white mb-4" style="line-height: 1.1;">Müşteri Aramayın, Bırakın <span class="text-warning">Dev Projeler</span> Sizi Bulsun.</h1>
                <p class="lead text-white-50 mb-4 pe-lg-5">
                    Dışarıda müşteri peşinde koşmayın. İnşaat malzemesinden hırdavata tüm ürünlerinizi GMK360'a ekleyin. Ekosistemdeki binlerce müteahhit, şantiye şefi ve tesis yöneticisinin açtığı malzeme talepleri (İhaleler) doğrudan ekranınıza düşsün! Üstelik sipariş ve stok operasyonlarınızı yöneteceğiniz altyapı da bizden.
                </p>'''

new_hero = '''<h1 class="display-4 fw-bolder text-white mb-4" style="line-height: 1.1;">Siz Müşteri Aramayın, Bırakın <span class="text-warning">Ustalar ve Müşteriler</span> Sizi Bulsun.</h1>
                <p class="lead text-white-50 mb-4 pe-lg-5">
                    Dışarıda müşteri peşinde koşmayın. İnşaat malzemesinden hırdavata tüm ürünlerinizi GMK360'a ekleyin. Ekosistemdeki binlerce mülk sahibi, usta ve dev şantiyelerin malzeme talepleri doğrudan ekranınıza düşsün! Üstelik perakende ve toptan siparişlerinizi yöneteceğiniz açık hesap altyapısı da bizden.
                </p>'''

content = content.replace(old_hero, new_hero)

with open(r'GMK360.Web\Views\Modules\Tedarik.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
