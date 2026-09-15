import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_ConstructionLayout.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

new_menu = '''<div class="list-group list-group-flush" style="font-size: 0.9rem;">
                    
                    <div class="list-group-item bg-light fw-bold text-uppercase" style="font-size: 0.8rem; color: #6c757d;">
                        ŞANTİYE & PROJELER
                    </div>
                    <a href="/ConstructionProject/Index" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Controller"]?.ToString() == "ConstructionProject" ? "active" : "")">
                        <i class="bi bi-buildings me-2 text-primary"></i> Projeler (Şantiyeler)
                    </a>
                    <a href="/DailyTimesheets/Index" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Controller"]?.ToString() == "DailyTimesheets" ? "active" : "")">
                        <i class="bi bi-clock-history me-2 text-primary"></i> Şantiye Puantaj Özeti
                    </a>
                    <a href="/Inventory/Index" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Controller"]?.ToString() == "Inventory" ? "active" : "")">
                        <i class="bi bi-box-seam me-2 text-primary"></i> Şantiye Depoları
                    </a>

                    <div class="list-group-item bg-light fw-bold mt-2 text-uppercase" style="font-size: 0.8rem; color: #6c757d;">
                        PERSONEL & EKİPLER
                    </div>
                    <a href="/AgencyStaff/Index" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Controller"]?.ToString() == "AgencyStaff" ? "active" : "")">
                        <i class="bi bi-person-badge me-2 text-info"></i> Beyaz Yaka (Merkez)
                    </a>
                    <a href="/AgencyWorkers/Index" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Controller"]?.ToString() == "AgencyWorkers" ? "active" : "")">
                        <i class="bi bi-person-workspace me-2 text-info"></i> Mavi Yaka (Şantiye)
                    </a>
                    <a href="/AgencyPhonebook/Index" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Controller"]?.ToString() == "AgencyPhonebook" ? "active" : "")">
                        <i class="bi bi-journal-bookmark-fill me-2 text-info"></i> Firma & Usta Rehberi
                    </a>

                    <div class="list-group-item bg-light fw-bold mt-2 text-uppercase" style="font-size: 0.8rem; color: #6c757d;">
                        FİNANS & CARİ HESAPLAR
                    </div>
                    <a href="/SupplierCurrentAccount/Index" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Controller"]?.ToString() == "SupplierCurrentAccount" ? "active" : "")">
                        <i class="bi bi-shop me-2 text-success"></i> Tedarikçi (Açık Hesap)
                    </a>
                    <a href="/SubcontractorContract/Index" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Controller"]?.ToString() == "SubcontractorContract" ? "active" : "")">
                        <i class="bi bi-hammer me-2 text-success"></i> Taşeron Hakedişleri
                    </a>
                    <a href="/Timesheet/PendingAdvances" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Controller"]?.ToString() == "Timesheet" ? "active fw-bold text-danger" : "")">
                        <i class="bi bi-cash-stack me-2 text-danger"></i> Personel Maaş & Avans
                    </a>
                    <a href="/Finance/Cashflow" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Action"]?.ToString() == "Cashflow" ? "active" : "")">
                        <i class="bi bi-bank me-2 text-success"></i> Ortak Kasa (Alacak/Verecek)
                    </a>
                    <a href="/Finance/UpcomingPayments" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Action"]?.ToString() == "UpcomingPayments" ? "active" : "")">
                        <i class="bi bi-calendar-event me-2 text-warning"></i> Vadesi Yaklaşanlar
                    </a>

                    <div class="list-group-item bg-light fw-bold mt-2 text-uppercase" style="font-size: 0.8rem; color: #6c757d;">
                        KURUMSAL & DİĞER
                    </div>
                    <a href="/B2BPurchasing/Index" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Controller"]?.ToString() == "B2BPurchasing" ? "active" : "")">
                        <i class="bi bi-cart3 me-2 text-secondary"></i> B2B Satın Alma
                    </a>
                    <a href="/DocumentArchive/Index?context=construction" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Controller"]?.ToString() == "DocumentArchive" ? "active" : "")">
                        <i class="bi bi-folder-fill me-2 text-secondary"></i> Arşiv & Sözleşmeler
                    </a>
                    <a href="/Dashboard/SiteYonetimi" class="list-group-item list-group-item-action">
                        <i class="bi bi-layout-text-window me-2 text-secondary"></i> Kurumsal Web Sitem (CMS)
                    </a>
                    
                    <a href="/Dashboard/Hub" class="list-group-item list-group-item-action bg-dark text-white mt-3 text-center rounded-bottom">
                        <i class="bi bi-grid-3x3-gap-fill me-2"></i> Tüm Uygulamalar (Hub)
                    </a>
                </div>'''

start_str = '<div class="list-group list-group-flush" style="font-size: 0.9rem;">'
# We will use regex to replace everything from start_str up to the first instance of "Tüm Uygulamalar (Hub)\r\n                    </a>\r\n                </div>"
pattern = re.compile(re.escape(start_str) + r'.*?Tüm Uygulamalar \(Hub\).*?</a>\s*</div>', re.DOTALL)

if pattern.search(content):
    content = pattern.sub(new_menu, content, count=1)
    with codecs.open(filepath, 'w', 'utf-8-sig') as f:
        f.write(content)
    print("Replaced successfully")
else:
    print("Pattern not found")

