import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_ConstructionLayout.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

# We need to replace the entire <div class="list-group list-group-flush ...">...</div> block 
# inside the col-xl-2 (which contains the card shadow-sm).
# The safest way is to use regex or string replace from <div class="list-group list-group-flush" to </div>\r\n            </div>\r\n        </div>

new_menu = '''<div class="list-group list-group-flush">
                    
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

pattern = re.compile(r'<div class="list-group list-group-flush">.*?</nav>', re.DOTALL) # wait, it might not end in </nav>
# Let's use string splitting based on known strings.

start_str = '<div class="list-group list-group-flush">'
end_str = '</a>\r\n                </div>\r\n            </div>\r\n        </div>' # The hub button is the last
end_str2 = '</a>\n                </div>\n            </div>\n        </div>'

if start_str in content:
    pre = content[:content.find(start_str)]
    
    # find the end
    end_idx = content.find(end_str)
    if end_idx != -1:
        post = content[end_idx + len('</a>\r\n                </div>'):]
    else:
        end_idx2 = content.find(end_str2)
        if end_idx2 != -1:
            post = content[end_idx2 + len('</a>\n                </div>'):]
        else:
            # Fallback regex
            post_match = re.search(r'</a>\s*</div>\s*</div>\s*</div>', content[content.find(start_str):])
            if post_match:
                post = content[content.find(start_str) + post_match.end() - len('\r\n            </div>\r\n        </div>'):]
            else:
                post = ''
    
    new_content = pre + new_menu + post
    with codecs.open(filepath, 'w', 'utf-8-sig') as f:
        f.write(new_content)
else:
    print("Could not find start string")

