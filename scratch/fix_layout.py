import re

with open('GMK360.Web/Views/Shared/_ConstructionLayout.cshtml', 'r', encoding='utf-8') as f:
    content = f.read()

old_kurumsal = '''<div id="colKurumsal" class="accordion-collapse collapse" data-bs-parent="#sidebarAccordion">
                                <div class="list-group list-group-flush" style="font-size: 0.9rem;">
                                    <a href="/ProjectFinance/Index" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-secondary"></i> Karar Destek</a>
                                    <a href="/B2BPurchasing/Index" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-secondary"></i> B2B Satın Alma</a>
                                </div>
                            </div>'''

new_kurumsal = '''<div id="colKurumsal" class="accordion-collapse collapse" data-bs-parent="#sidebarAccordion">
                                <div class="list-group list-group-flush" style="font-size: 0.9rem;">
                                    <a href="/ProjectFinance/Index" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-secondary"></i> Karar Destek</a>
                                    <a href="/B2BPurchasing/Index" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-secondary"></i> B2B Satın Alma</a>
                                    <a href="/DocumentArchives/Index" class="list-group-item list-group-item-action border-0 ps-5 fw-bold text-dark"><i class="bi bi-arrow-right-short text-secondary"></i> Evrak Belge Arşivi</a>
                                    <a href="/AgencyWeb/Index" class="list-group-item list-group-item-action border-0 ps-5 fw-bold text-dark"><i class="bi bi-arrow-right-short text-secondary"></i> Kurumsal Web Sitesi Yönetimi</a>
                                </div>
                            </div>'''

if old_kurumsal in content:
    content = content.replace(old_kurumsal, new_kurumsal)
    with open('GMK360.Web/Views/Shared/_ConstructionLayout.cshtml', 'w', encoding='utf-8') as f:
        f.write(content)
    print("Fixed layout!")
else:
    print("Not found, try a looser match.")
