import io
import re

filepath = r'GMK360.Web\Views\B2BPurchasing\Details.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# I want to inject the recommendation card right after the entire "Alınan Teklifler" card.
# The card ends exactly before a new <div class="row"> or something, or it's the last card.
# Let's search for: 
#                     }
#                 </div>
#             </div>
#         </div>

# A reliable way: append at the very end of the main container, but before the <script> block if there is one.
# Let's see the end of the file.
lines = content.split('\n')
script_idx = len(lines)
for i, line in enumerate(lines):
    if '@section Scripts' in line:
        script_idx = i
        break

recommendation_html = """
    <!-- AKILLI PAZAR YERİ ÖNERİ MOTORU -->
    <div class="row mt-4">
        <div class="col-12">
            <div class="card shadow-sm border-0 bg-white" style="border-radius: 12px; overflow:hidden;">
                <div class="card-header border-0 py-3 d-flex justify-content-between align-items-center" style="background: linear-gradient(135deg, #1e3a8a, #3b82f6); color: white;">
                    <div class="fw-bold fs-5">
                        <i class="bi bi-stars text-warning me-2"></i> Pazar Yerinden Önerilen Firmalar
                    </div>
                    <span class="badge bg-white text-primary rounded-pill px-3 py-2">GMK360 Akıllı Algoritma</span>
                </div>
                <div class="card-body bg-light p-4">
                    <p class="text-muted small mb-4">
                        <i class="bi bi-info-circle me-1"></i> Sizin eklediğiniz firmalar dışında, aradığınız <strong>@Model.Title</strong> kategorisinde sistemimizde doğrulanmış ve yüksek puanlı bölgesel firmalar aşağıda listelenmiştir. Tek tıkla ihaleye davet edebilirsiniz.
                    </p>
                    
                    <div class="row g-3">
                        <!-- Öneri 1 -->
                        <div class="col-md-4">
                            <div class="card h-100 border-0 shadow-sm" style="border-radius:10px; transition: transform 0.2s;">
                                <div class="card-body position-relative">
                                    <div class="position-absolute top-0 end-0 mt-2 me-2">
                                        <span class="badge bg-success bg-opacity-10 text-success rounded-pill"><i class="bi bi-shield-check"></i> Doğrulanmış</span>
                                    </div>
                                    <h6 class="fw-bold mb-1 mt-2">Zirve Yapı ve Zemin Laboratuvarı</h6>
                                    <div class="text-warning small mb-2">
                                        <i class="bi bi-star-fill"></i><i class="bi bi-star-fill"></i><i class="bi bi-star-fill"></i><i class="bi bi-star-fill"></i><i class="bi bi-star-half"></i> 
                                        <span class="text-muted ms-1">(4.8 / 12 İşlem)</span>
                                    </div>
                                    <p class="small text-muted mb-3"><i class="bi bi-geo-alt"></i> Merkez, Ana Bölge</p>
                                    
                                    <button class="btn btn-sm btn-outline-primary w-100 rounded-pill fw-bold" onclick="alert('Pazar Yeri entegrasyonu sağlandığında firma otomatik davet edilecektir.')">
                                        <i class="bi bi-send me-1"></i> Bunu da Davet Et
                                    </button>
                                </div>
                            </div>
                        </div>
                        
                        <!-- Öneri 2 -->
                        <div class="col-md-4">
                            <div class="card h-100 border-0 shadow-sm" style="border-radius:10px; transition: transform 0.2s;">
                                <div class="card-body position-relative">
                                    <div class="position-absolute top-0 end-0 mt-2 me-2">
                                        <span class="badge bg-success bg-opacity-10 text-success rounded-pill"><i class="bi bi-shield-check"></i> Doğrulanmış</span>
                                    </div>
                                    <h6 class="fw-bold mb-1 mt-2">Güven Karot ve Beton Delme</h6>
                                    <div class="text-warning small mb-2">
                                        <i class="bi bi-star-fill"></i><i class="bi bi-star-fill"></i><i class="bi bi-star-fill"></i><i class="bi bi-star-fill"></i><i class="bi bi-star-fill"></i> 
                                        <span class="text-muted ms-1">(5.0 / 8 İşlem)</span>
                                    </div>
                                    <p class="small text-muted mb-3"><i class="bi bi-geo-alt"></i> Merkez, Ana Bölge</p>
                                    
                                    <button class="btn btn-sm btn-outline-primary w-100 rounded-pill fw-bold" onclick="alert('Pazar Yeri entegrasyonu sağlandığında firma otomatik davet edilecektir.')">
                                        <i class="bi bi-send me-1"></i> Bunu da Davet Et
                                    </button>
                                </div>
                            </div>
                        </div>
                        
                        <!-- Öneri 3 -->
                        <div class="col-md-4">
                            <div class="card h-100 border-0 shadow-sm" style="border-radius:10px; transition: transform 0.2s;">
                                <div class="card-body position-relative">
                                    <div class="position-absolute top-0 end-0 mt-2 me-2">
                                        <span class="badge bg-secondary bg-opacity-10 text-secondary rounded-pill"><i class="bi bi-clock-history"></i> Yeni Katıldı</span>
                                    </div>
                                    <h6 class="fw-bold mb-1 mt-2">Teknik Denetim Hizmetleri A.Ş.</h6>
                                    <div class="text-warning small mb-2">
                                        <i class="bi bi-star text-muted"></i><i class="bi bi-star text-muted"></i><i class="bi bi-star text-muted"></i><i class="bi bi-star text-muted"></i><i class="bi bi-star text-muted"></i> 
                                        <span class="text-muted ms-1">(Henüz değerlendirilmedi)</span>
                                    </div>
                                    <p class="small text-muted mb-3"><i class="bi bi-geo-alt"></i> Merkez, Ana Bölge</p>
                                    
                                    <button class="btn btn-sm btn-outline-primary w-100 rounded-pill fw-bold" onclick="alert('Pazar Yeri entegrasyonu sağlandığında firma otomatik davet edilecektir.')">
                                        <i class="bi bi-send me-1"></i> Bunu da Davet Et
                                    </button>
                                </div>
                            </div>
                        </div>
                        
                    </div>
                </div>
            </div>
        </div>
    </div>
"""

lines.insert(script_idx, recommendation_html)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write("\n".join(lines))
    
print("Injected Recommendations")
