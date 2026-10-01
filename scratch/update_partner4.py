import io
import re

filepath = r'GMK360.Web\Views\B2BPartnerPortal\Invite.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

injection = """
                        <div class="list-group list-group-flush small">
                            @{
                                bool isLab = Model.QuoteRequest?.Title != null && (Model.QuoteRequest.Title.IndexOf("Karot", StringComparison.OrdinalIgnoreCase) >= 0 || Model.QuoteRequest.Title.IndexOf("Laboratuvar", StringComparison.OrdinalIgnoreCase) >= 0 || Model.QuoteRequest.Title.IndexOf("Zemin", StringComparison.OrdinalIgnoreCase) >= 0 || Model.QuoteRequest.Title.IndexOf("Tespit Raporu", StringComparison.OrdinalIgnoreCase) >= 0);
                            }
                            <a href="#" class="list-group-item list-group-item-action d-flex justify-content-between align-items-center py-3 border-0 active fw-bold shadow-sm rounded-3 mb-2" style="background: linear-gradient(135deg, #2563eb, #1e40af); color: white;">
                                <span><i class="bi bi-wallet2 text-warning me-2 fs-5 align-middle"></i> @(Model.QuoteRequest?.RequesterAgency?.CompanyName ?? "Firma") İle Cari Bakiyesi</span>
                                <span class="badge bg-white text-primary rounded-pill px-3 shadow-sm">Ücretsiz</span>
                            </a>
                            
                            @if(isLab)
                            {
                                <a href="#" class="list-group-item list-group-item-action d-flex justify-content-between align-items-center py-3 border-0 text-dark fw-bold rounded-3 mb-2" style="background-color: #f8f9fa;">
                                    <span><i class="bi bi-file-earmark-pdf-fill text-danger me-2 fs-5 align-middle"></i> Test/Karot Raporunu Yükle</span>
                                    <span class="badge bg-success rounded-pill px-3 shadow-sm">Aktif İşlem</span>
                                </a>
                                <a href="#" class="list-group-item list-group-item-action d-flex justify-content-between align-items-center py-3 border-0 text-muted">
                                    <span><i class="bi bi-journal-check text-secondary me-2 fs-5 align-middle"></i> Hizmet ve Saha Tutanağı</span>
                                    <span class="pro-badge shadow-sm"><i class="bi bi-lock-fill"></i> PRO</span>
                                </a>
                            }
                            else
                            {
                                <a href="#" class="list-group-item list-group-item-action d-flex justify-content-between align-items-center py-3 border-0 text-muted">
                                    <span><i class="bi bi-recycle text-secondary me-2 fs-5 align-middle"></i> Hurda / Çıkma Malzeme Envanteri</span>
                                    <span class="pro-badge shadow-sm"><i class="bi bi-lock-fill"></i> PRO</span>
                                </a>
                                <a href="#" class="list-group-item list-group-item-action d-flex justify-content-between align-items-center py-3 border-0 text-muted">
                                    <span><i class="bi bi-people-fill text-secondary me-2 fs-5 align-middle"></i> Saha Ekipleri (Puantaj)</span>
                                    <span class="pro-badge shadow-sm"><i class="bi bi-lock-fill"></i> PRO</span>
                                </a>
                                <a href="#" class="list-group-item list-group-item-action d-flex justify-content-between align-items-center py-3 border-0 text-muted">
                                    <span><i class="bi bi-truck text-secondary me-2 fs-5 align-middle"></i> Sefer ve Kantar Fişleri</span>
                                    <span class="pro-badge shadow-sm"><i class="bi bi-lock-fill"></i> PRO</span>
                                </a>
                                <a href="#" class="list-group-item list-group-item-action d-flex justify-content-between align-items-center py-3 border-0 text-muted">
                                    <span><i class="bi bi-truck-front-fill text-secondary me-2 fs-5 align-middle"></i> Makine ve Araç Filom</span>
                                    <span class="pro-badge shadow-sm"><i class="bi bi-lock-fill"></i> PRO</span>
                                </a>
                                <a href="#" class="list-group-item list-group-item-action d-flex justify-content-between align-items-center py-3 border-0 text-muted">
                                    <span><i class="bi bi-fuel-pump-fill text-secondary me-2 fs-5 align-middle"></i> Yakıt (Mazot) Takibi</span>
                                    <span class="pro-badge shadow-sm"><i class="bi bi-lock-fill"></i> PRO</span>
                                </a>
                            }
                        </div>"""

start_idx = content.find('<div class="list-group list-group-flush small">')
end_idx = content.find('</div>', content.find('PRO</span>', start_idx)) + 6
# since there are multiple PRO</span>, let's find the last one by looping or finding the start of the next div
next_div_idx = content.find('<div class="mt-4 text-center">', start_idx)
if next_div_idx != -1:
    content = content[:start_idx] + injection + content[next_div_idx:]
    with io.open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
    print("Successfully replaced.")
else:
    print("Indices not found.")
