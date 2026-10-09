import re

with open(r'GMK360.Web\Views\Shared\_ConstructionLayout.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Replace the entire <div class="col-lg-3 col-xl-2 border-end bg-white p-0 d-none d-lg-block"> block
pattern = r'(<div class="col-lg-3 col-xl-2 border-end bg-white p-0 d-none d-lg-block".*?>).*?(<!-- SAĞ: İÇERİK ALANI -->)'

new_sidebar = '''\\1
            <div class="d-flex flex-column h-100 position-sticky" style="top: 76px; height: calc(100vh - 76px) !important;">
                <div class="bg-primary text-white p-3 fw-bold shadow-sm d-flex justify-content-between align-items-center">
                    <div><i class="bi bi-cone-striped me-2"></i> Şantiye & İnşaat ERP</div>
                </div>
                <div class="card-body p-0" style="overflow-y: auto; overflow-x: hidden;">
                    
                    <div class="accordion accordion-flush" id="sidebarAccordion">
                        
                        <!-- 1. ŞANTİYE & PROJELER -->
                        <div class="accordion-item border-0">
                            <h2 class="accordion-header">
                                <button class="accordion-button collapsed py-3 fw-bold bg-light" type="button" data-bs-toggle="collapse" data-bs-target="#colProjeler">
                                    <i class="bi bi-buildings text-primary me-2"></i> ŞANTİYE & PROJELER
                                </button>
                            </h2>
                            <div id="colProjeler" class="accordion-collapse collapse" data-bs-parent="#sidebarAccordion">
                                <div class="list-group list-group-flush" style="font-size: 0.9rem;">
                                    <a href="/ConstructionProject/Index" class="list-group-item list-group-item-action border-0 ps-4"><i class="bi bi-arrow-right-short text-primary"></i> Projeler (Şantiyeler)</a>
                                    <a href="/DailyTimesheets/Index" class="list-group-item list-group-item-action border-0 ps-4"><i class="bi bi-arrow-right-short text-primary"></i> Şantiye Puantaj</a>
                                    <a href="/Inventory/Index" class="list-group-item list-group-item-action border-0 ps-4"><i class="bi bi-arrow-right-short text-primary"></i> Merkez Depo (Demirbaş)</a>
                                </div>
                            </div>
                        </div>

                        <!-- 2. PERSONEL & EKİPLER -->
                        <div class="accordion-item border-0">
                            <h2 class="accordion-header">
                                <button class="accordion-button collapsed py-3 fw-bold bg-light" type="button" data-bs-toggle="collapse" data-bs-target="#colPersonel">
                                    <i class="bi bi-people text-info me-2"></i> PERSONEL & EKİPLER
                                </button>
                            </h2>
                            <div id="colPersonel" class="accordion-collapse collapse" data-bs-parent="#sidebarAccordion">
                                <div class="list-group list-group-flush" style="font-size: 0.9rem;">
                                    <a href="/AgencyStaff/Index" class="list-group-item list-group-item-action border-0 ps-4"><i class="bi bi-arrow-right-short text-info"></i> Beyaz Yaka (Merkez)</a>
                                    <a href="/AgencyWorkers/Index" class="list-group-item list-group-item-action border-0 ps-4"><i class="bi bi-arrow-right-short text-info"></i> Mavi Yaka (Şantiye)</a>
                                    <a href="/AgencyPhonebook/Index" class="list-group-item list-group-item-action border-0 ps-4"><i class="bi bi-arrow-right-short text-info"></i> Firma & Usta Rehberi</a>
                                </div>
                            </div>
                        </div>

                        <!-- 3. FİNANS & CARİ HESAPLAR -->
                        <div class="accordion-item border-0">
                            <h2 class="accordion-header">
                                <button class="accordion-button collapsed py-3 fw-bold bg-light" type="button" data-bs-toggle="collapse" data-bs-target="#colFinans">
                                    <i class="bi bi-cash-coin text-success me-2"></i> FİNANS & CARİ
                                </button>
                            </h2>
                            <div id="colFinans" class="accordion-collapse collapse" data-bs-parent="#sidebarAccordion">
                                <div class="list-group list-group-flush" style="font-size: 0.9rem;">
                                    <a href="/SupplierCurrentAccount/Index" class="list-group-item list-group-item-action border-0 ps-4"><i class="bi bi-arrow-right-short text-success"></i> Tedarikçi (Açık Hesap)</a>
                                    <a href="/SubcontractorContract/Index" class="list-group-item list-group-item-action border-0 ps-4"><i class="bi bi-arrow-right-short text-success"></i> Taşeron Hakedişleri</a>
                                    <a href="/Timesheet/PendingAdvances" class="list-group-item list-group-item-action border-0 ps-4 fw-bold text-danger"><i class="bi bi-cash-stack text-danger"></i> Personel Maaş & Avans</a>
                                    <a href="/Finance/Cashflow" class="list-group-item list-group-item-action border-0 ps-4"><i class="bi bi-arrow-right-short text-success"></i> Ortak Kasa</a>
                                </div>
                            </div>
                        </div>

                        <!-- 4. CRM & OPERASYON -->
                        <div class="accordion-item border-0">
                            <h2 class="accordion-header">
                                <button class="accordion-button collapsed py-3 fw-bold bg-light" type="button" data-bs-toggle="collapse" data-bs-target="#colCrm">
                                    <i class="bi bi-telephone-inbound text-warning me-2"></i> CRM & OPERASYON
                                </button>
                            </h2>
                            <div id="colCrm" class="accordion-collapse collapse" data-bs-parent="#sidebarAccordion">
                                <div class="list-group list-group-flush" style="font-size: 0.9rem;">
                                    <a href="/ProjectCrm/Index" class="list-group-item list-group-item-action border-0 ps-4"><i class="bi bi-arrow-right-short text-warning"></i> Potansiyel Alıcı & CRM</a>
                                    <a href="/Meetings/Index" class="list-group-item list-group-item-action border-0 ps-4"><i class="bi bi-arrow-right-short text-warning"></i> Toplantı & Ajanda</a>
                                    <a href="/CompanyVehicles/Index" class="list-group-item list-group-item-action border-0 ps-4"><i class="bi bi-arrow-right-short text-warning"></i> Şirket Garajı & Araçlar</a>
                                    <a href="/ConstructionProjectExpenses/Index" class="list-group-item list-group-item-action border-0 ps-4"><i class="bi bi-arrow-right-short text-danger"></i> Şirket Genel Giderleri</a>
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
                                    <a href="/ProjectFinance/Index" class="list-group-item list-group-item-action border-0 ps-4"><i class="bi bi-arrow-right-short text-secondary"></i> Karar Destek</a>
                                    <a href="/B2BPurchasing/Index" class="list-group-item list-group-item-action border-0 ps-4"><i class="bi bi-arrow-right-short text-secondary"></i> B2B Satın Alma</a>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        \\2'''
        
content = re.sub(pattern, new_sidebar, content, flags=re.DOTALL)

# Now move "Tüm Uygulamalar" to the Topbar next to the Weather!
# We find <!-- Hava Durumu (Bölge Seçimli) --> and insert the Hub button next to it.
old_topbar = '''<!-- Hava Durumu (Bölge Seçimli) -->'''
new_topbar = '''<!-- Tüm Uygulamalar (Hub) Butonu -->
                <a href="/Dashboard/Hub" class="btn btn-dark rounded-pill shadow-sm d-flex align-items-center gap-2 me-2 px-3 border border-secondary" title="Tüm Uygulamalara Dön">
                    <i class="bi bi-grid-3x3-gap-fill text-white"></i> <span class="d-none d-xl-inline" style="font-size: 0.85rem;">Hub</span>
                </a>
                
                <!-- Hava Durumu (Bölge Seçimli) -->'''
content = content.replace(old_topbar, new_topbar)

with open(r'GMK360.Web\Views\Shared\_ConstructionLayout.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
