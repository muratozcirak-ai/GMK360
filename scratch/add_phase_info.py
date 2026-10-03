import codecs
import re

path = 'GMK360.Web/Views/PhaseOne/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'(<h3 class="fw-bold text-dark mb-0"><i class="bi bi-cone-striped text-primary me-2"></i> FAZ 1: Yıkım ve Zemin Hazırlığı \(Fizibilite Bütçesi\)</h3>\s*<p class="text-muted mt-1 mb-0">Şantiye öncesi bütçeleme ve planlama aşaması \(Zaman çizelgesi ve puantaj yönetimi inşaat başlayınca aktif olacaktır\)\.</p>\s*</div>\s*<div class="text-end">\s*<form action="/PhaseOne/SyncFromPool/@projectId" method="post" class="d-inline me-3">\s*<button type="submit" class="btn btn-warning fw-bold shadow-sm rounded-pill px-4"><i class="bi bi-cloud-download me-1"></i> Havuzdan Senkronize Et</button>\s*</form>\s*<h4 class="fw-bold text-primary mb-0 d-inline-block">@Model.Sum\(x => x.PlannedTotalCost\).ToString\("N2"\) ₺ <span class="fs-6 text-muted fw-normal">Toplam Fizibilite Bütçesi</span></h4>\s*</div>\s*</div>)'

replacement = r'''\1

    <div class="alert alert-info border-0 shadow-sm rounded-4 mb-4 d-flex align-items-center">
        <i class="bi bi-lightbulb-fill fs-3 text-info me-3"></i>
        <div>
            <h6 class="fw-bold mb-1">Bu Liste Nasıl Kullanılır?</h6>
            <p class="mb-0 small text-dark">
                Havuzdan senkronize edilen kalemler, projede unutulmaması gereken <strong>asgari ve mecburi</strong> temel işlerdir (Abonelikler, ruhsatlar vb.). 
                Şantiyenizin özel fiziki şartlarına (örn: dağ başında olduğu için su tankeri gerekmesi) veya şirketinizin demirbaş durumuna (örn: JCB veya Konteyneriniz yoksa kiralama gerekmesi) göre doğacak ek ihtiyaçları, ilgili kategorinin altındaki <strong>"Yeni Bütçe Kalemi Ekle"</strong> butonunu kullanarak fizibilitenize eklemelisiniz.
            </p>
        </div>
    </div>'''

content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)