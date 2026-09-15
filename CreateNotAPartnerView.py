import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\PartnerPortal\NotAPartner.cshtml'

content = '''@{
    ViewData["Title"] = "Erişim Yok";
}

<div class="container text-center py-5 mt-5">
    <i class="bi bi-person-x display-1 text-muted mb-3 d-block opacity-50"></i>
    <h2 class="fw-bold">Bu Sayfaya Erişiminiz Yok</h2>
    <p class="text-muted">
        Bu ekranlar yalnızca sistemimize kayıtlı <strong>Tedarikçi ve Taşeron</strong> iş ortaklarımız içindir.<br />
        Eğer bir iş ortağı olduğunuzu düşünüyorsanız, lütfen şirket yöneticinizden size bir 'Gölge Kullanıcı' yetkisi tanımlamasını talep edin.
    </p>
    <a href="/Dashboard/Index" class="btn btn-primary mt-3"><i class="bi bi-house me-2"></i> Ana Sayfaya Dön</a>
</div>
'''

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
