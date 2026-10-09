import re
with open(r'GMK360.Web\Views\Modules\Usta.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

old_hero = '''<span class="badge bg-primary px-3 py-2 rounded-pill mb-3 fw-bold tracking-wide">TAŞERON ERP'Sİ & HİZMET AĞI</span>
                <h1 class="display-4 fw-bolder text-white mb-4" style="line-height: 1.1;">Muhasebe Değil, <span class="text-primary">Dijital Şantiye Defteri.</span></h1>
                <p class="lead text-white-50 mb-4 pe-lg-5">
                    "İşi kaça aldım, bugüne kadar ne kadar masraf ettim, cebime ne kalacak?" sorusunun cevabını karmaşık menülerde kaybolmadan, bakkal defteri basitliğinde tek ekranda görün.
                </p>
                <div class="d-flex gap-3">
                    <button class="btn btn-primary btn-lg px-5 rounded-pill fw-bold shadow-lg hover-lift">Ekibinizi Dijitale Taşıyın</button>
                </div>'''

new_hero = '''<span class="badge bg-primary px-3 py-2 rounded-pill mb-3 fw-bold tracking-wide">TÜRKİYE'NİN EN BÜYÜK HİZMET AĞI</span>
                <h1 class="display-4 fw-bolder text-white mb-4" style="line-height: 1.1;">Müşteri Aramayın, Bırakın <span class="text-primary">İş Fırsatları</span> Sizi Bulsun.</h1>
                <p class="lead text-white-50 mb-4 pe-lg-5">
                    Usta veya müşteri aramakla vakit kaybetmeyin. Ekosistemdeki binlerce mülk sahibi, şantiye ve site yöneticisinin açtığı iş ilanları doğrudan cebinize düşsün. Üstelik aldığınız işleri, ekiplerinizi ve kârınızı yöneteceğiniz "Taşeron ERP'si" de platformun hediyesi!
                </p>
                <div class="d-flex gap-3">
                    <button class="btn btn-primary btn-lg px-5 rounded-pill fw-bold shadow-lg hover-lift">Hizmet Ağına Katılın</button>
                </div>'''

content = content.replace(old_hero, new_hero)

with open(r'GMK360.Web\Views\Modules\Usta.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
