import re

with open(r'GMK360.Web\Views\Shared\_ConstructionLayout.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Add keyframes pulse to CSS
pulse_css = '''
        @keyframes pulse {
            0% { box-shadow: 0 0 0 0 rgba(220, 53, 69, 0.7); }
            70% { box-shadow: 0 0 0 6px rgba(220, 53, 69, 0); }
            100% { box-shadow: 0 0 0 0 rgba(220, 53, 69, 0); }
        }
    </style>'''
content = content.replace('</style>', pulse_css)

# Inject Notification Bell right before Hava Durumu
hava_durumu = '<!-- Hava Durumu (Bölge Seçimli) -->'

bell_html = '''<!-- Acil Durum / Bildirim Zili -->
                <div class="dropdown me-1">
                    <button class="btn btn-light rounded-circle shadow-sm position-relative d-flex justify-content-center align-items-center" type="button" data-bs-toggle="dropdown" aria-expanded="false" style="width: 40px; height: 40px;" title="Acil Durumlar & Bildirimler">
                        <i class="bi bi-bell-fill text-secondary fs-5"></i>
                        <span class="position-absolute top-0 start-100 translate-middle p-1 bg-danger border border-light rounded-circle" style="animation: pulse 1.5s infinite;">
                            <span class="visually-hidden">Yeni Bildirimler</span>
                        </span>
                    </button>
                    <ul class="dropdown-menu dropdown-menu-end shadow-lg border-0 mt-3 p-0" style="border-radius: 12px; min-width: 350px;">
                        <li class="bg-danger text-white px-4 py-3 fw-bold rounded-top-3 d-flex justify-content-between align-items-center" style="border-top-left-radius: 12px; border-top-right-radius: 12px;">
                            <span><i class="bi bi-exclamation-triangle-fill me-2"></i> Acil Şantiye Bildirimleri</span>
                            <span class="badge bg-light text-danger rounded-pill">2 Yeni</span>
                        </li>
                        <li class="list-group list-group-flush">
                            <a href="#" class="list-group-item list-group-item-action p-3 border-bottom border-light">
                                <div class="d-flex w-100 justify-content-between mb-1">
                                    <h6 class="mb-0 fw-bold text-danger"><i class="bi bi-cone-striped me-1"></i> Beton Pompası Arızası</h6>
                                    <small class="text-muted">10 dk önce</small>
                                </div>
                                <p class="mb-0 small text-dark">Beyaz Konak şantiyesinde döküm durdu. Acil müdahale ekibi bekleniyor.</p>
                            </a>
                            <a href="#" class="list-group-item list-group-item-action p-3">
                                <div class="d-flex w-100 justify-content-between mb-1">
                                    <h6 class="mb-0 fw-bold text-warning"><i class="bi bi-box-seam me-1"></i> Demir Stoğu Kritik</h6>
                                    <small class="text-muted">1 saat önce</small>
                                </div>
                                <p class="mb-0 small text-dark">Merkez Depo'da 14'lük nervürlü demir seviyesi minimumun altına düştü.</p>
                            </a>
                        </li>
                        <li class="bg-light px-3 py-2 text-center rounded-bottom-3 border-top" style="border-bottom-left-radius: 12px; border-bottom-right-radius: 12px;">
                            <a href="#" class="text-decoration-none fw-bold small text-primary">Tüm Acil Durumları Gör</a>
                        </li>
                    </ul>
                </div>

                '''

content = content.replace(hava_durumu, bell_html + hava_durumu)

with open(r'GMK360.Web\Views\Shared\_ConstructionLayout.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
