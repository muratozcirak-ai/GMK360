import re

with open(r'GMK360.Web\Views\Shared\_ConstructionLayout.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

start_tag = '<div class="card shadow-sm border-0">'
end_tag = '<!-- SAĞ: PROFİL MENÜSÜ -->' # wait, no. The list group ends before @{

start_idx = content.find(start_tag)
end_idx = content.find('</div>\n        </div>\n\n        \n@{')
if end_idx == -1:
    end_idx = content.find('</div>\n        </div>\n\n@{')

if start_idx != -1 and end_idx != -1:
    new_sidebar = '''<div class="card shadow-sm border-0 position-sticky" style="top: 90px; max-height: calc(100vh - 100px); overflow-y: auto;">
                <div class="card-header bg-primary text-white fw-bold py-3">
                    <i class="bi bi-cone-striped me-2"></i> Şantiye & İnşaat ERP
                </div>
                <div class="card-body p-0">
                    <div class="accordion accordion-flush" id="sidebarAccordion">
                        
                        <!-- 1. ŞANTİYE & PROJELER -->
                        <div class="accordion-item border-0 border-bottom">
                            <h2 class="accordion-header">
                                <button class="accordion-button py-3 fw-bold bg-light" type="button" data-bs-toggle="collapse" data-bs-target="#colProjeler" aria-expanded="true">
                                    <i class="bi bi-buildings text-primary me-2"></i> ŞANTİYE & PROJELER
                                </button>
                            </h2>
                            <div id="colProjeler" class="accordion-collapse collapse show" data-bs-parent="#sidebarAccordion">
                                <div class="list-group list-group-flush" style="font-size: 0.9rem;">
                                    <a href="/ConstructionProject/Index" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-primary"></i> Projeler (Şantiyeler)</a>
                                    <a href="/DailyTimesheets/Index" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-primary"></i> Şantiye Puantaj</a>
                                    <a href="/Inventory/Index" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-primary"></i> Merkez Depo (Demirbaş)</a>
                                </div>
                            </div>
                        </div>

                        <!-- 2. PERSONEL & EKİPLER -->
                        <div class="accordion-item border-0 border-bottom">
                            <h2 class="accordion-header">
                                <button class="accordion-button collapsed py-3 fw-bold bg-light" type="button" data-bs-toggle="collapse" data-bs-target="#colPersonel">
                                    <i class="bi bi-people text-info me-2"></i> PERSONEL & EKİPLER
                                </button>
                            </h2>
                            <div id="colPersonel" class="accordion-collapse collapse" data-bs-parent="#sidebarAccordion">
                                <div class="list-group list-group-flush" style="font-size: 0.9rem;">
                                    <a href="/AgencyStaff/Index" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-info"></i> Beyaz Yaka (Merkez)</a>
                                    <a href="/AgencyWorkers/Index" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-info"></i> Mavi Yaka (Şantiye)</a>
                                    <a href="/AgencyPhonebook/Index" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-info"></i> Firma & Usta Rehberi</a>
                                </div>
                            </div>
                        </div>

                        <!-- 3. FİNANS & CARİ HESAPLAR -->
                        <div class="accordion-item border-0 border-bottom">
                            <h2 class="accordion-header">
                                <button class="accordion-button collapsed py-3 fw-bold bg-light" type="button" data-bs-toggle="collapse" data-bs-target="#colFinans">
                                    <i class="bi bi-cash-coin text-success me-2"></i> FİNANS & CARİ
                                </button>
                            </h2>
                            <div id="colFinans" class="accordion-collapse collapse" data-bs-parent="#sidebarAccordion">
                                <div class="list-group list-group-flush" style="font-size: 0.9rem;">
                                    <a href="/SupplierCurrentAccount/Index" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-success"></i> Tedarikçi (Açık Hesap)</a>
                                    <a href="/SubcontractorContract/Index" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-success"></i> Taşeron Hakedişleri</a>
                                    <a href="/Timesheet/PendingAdvances" class="list-group-item list-group-item-action border-0 ps-5 fw-bold text-danger"><i class="bi bi-cash-stack text-danger"></i> Personel Maaş & Avans</a>
                                    <a href="/Finance/Cashflow" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-success"></i> Ortak Kasa</a>
                                    <a href="/Finance/UpcomingPayments" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-warning"></i> Vadesi Yaklaşanlar</a>
                                </div>
                            </div>
                        </div>

                        <!-- 4. CRM & OPERASYON -->
                        <div class="accordion-item border-0 border-bottom">
                            <h2 class="accordion-header">
                                <button class="accordion-button collapsed py-3 fw-bold bg-light" type="button" data-bs-toggle="collapse" data-bs-target="#colCrm">
                                    <i class="bi bi-telephone-inbound text-warning me-2"></i> CRM & OPERASYON
                                </button>
                            </h2>
                            <div id="colCrm" class="accordion-collapse collapse" data-bs-parent="#sidebarAccordion">
                                <div class="list-group list-group-flush" style="font-size: 0.9rem;">
                                    <a href="/ProjectCrm/Index" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-warning"></i> Potansiyel Alıcı & CRM</a>
                                    <a href="/Meetings/Index" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-warning"></i> Toplantı & Ajanda</a>
                                    <a href="/CompanyVehicles/Index" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-warning"></i> Şirket Garajı & Araçlar</a>
                                    <a href="/ConstructionProjectExpenses/Index" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-danger"></i> Şirket Genel Giderleri</a>
                                </div>
                            </div>
                        </div>

                        <!-- 5. KURUMSAL & DİĞER -->
                        <div class="accordion-item border-0">
                            <h2 class="accordion-header">
                                <button class="accordion-button collapsed py-3 fw-bold bg-light" type="button" data-bs-toggle="collapse" data-bs-target="#colKurumsal">
                                    <i class="bi bi-briefcase text-secondary me-2"></i> KURUMSAL
                                </button>
                            </h2>
                            <div id="colKurumsal" class="accordion-collapse collapse" data-bs-parent="#sidebarAccordion">
                                <div class="list-group list-group-flush" style="font-size: 0.9rem;">
                                    <a href="/ProjectFinance/Index" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-secondary"></i> Karar Destek</a>
                                    <a href="/B2BPurchasing/Index" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-secondary"></i> B2B Satın Alma</a>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            '''
    content = content[:start_idx] + new_sidebar + content[end_idx:]

with open(r'GMK360.Web\Views\Shared\_ConstructionLayout.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
