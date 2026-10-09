import re

with open('GMK360.Web/Views/ConstructionProject/Create.cshtml', 'r', encoding='utf-8') as f:
    content = f.read()

def replace_razor_old(match):
    return '''<h5 class="fw-bold text-danger border-bottom pb-2 mb-3"><i class="ph ph-building me-2"></i> @(i+1). Eski Yapı</h5>
                                                <div class="row g-3">
                                                    <div class="col-md-12 col-lg-4">
                                                        <label class="form-label small fw-bold">Eski Yapı Adı</label>
                                                        <input type="text" name="ExistingBlocks[@i].BlockName" class="form-control fw-bold" value="@b.BlockName" required />
                                                    </div>
                                                    <div class="col-12 col-lg-4">
                                                        <label class="form-label small fw-bold">Temel / Baza Durumu</label>
                                                        <select name="ExistingBlocks[@i].LayoutPattern" class="form-select form-select-sm mt-1" onchange="handleLayoutPatternChange(this)">
                                                            <option value="" selected="@(string.IsNullOrEmpty(b.LayoutPattern))">Mimari Tarz (Seçiniz)</option>
                                                            <option value="Tekil Yapı" selected="@(b.LayoutPattern == "Tekil Yapı")">Tekil Yapı (Standart)</option>
                                                            <option value="Bitişik Nizam" selected="@(b.LayoutPattern == "Bitişik Nizam")">Bitişik Nizam (Tevhit)</option>
                                                            <option value="Ortak Baza" selected="@(b.LayoutPattern == "Ortak Baza")">Ortak Baza (Kule Altı)</option>
                                                            <option value="Kule" selected="@(b.LayoutPattern == "Kule")">Kule (Baza Üstü)</option>
                                                        </select>
                                                    </div>
                                                    <div class="col-12 col-lg-4">
                                                        <label class="form-label small fw-bold">Taban (m²)</label>
                                                        <input type="number" name="ExistingBlocks[@i].BaseArea" class="form-control" placeholder="Örn: 200" min="1" value="@b.BaseArea" required />
                                                    </div>
                                                </div>
                                                
                                                <div class="row g-3 mt-3 tech-details" style="display: none; padding-top: 15px; border-top: 1px dashed #ccc;">
                                                    <div class="col-6 col-lg-2">
                                                        <label class="form-label small fw-bold">Kat Sayısı</label>
                                                        <input type="number" name="ExistingBlocks[@i].TotalFloors" class="form-control" min="1" value="@b.TotalFloors" required />
                                                    </div>
                                                    <div class="col-6 col-lg-2">
                                                        <label class="form-label small fw-bold">Daire</label>
                                                        <input type="number" name="ExistingBlocks[@i].TotalApartments" class="form-control" value="@b.TotalApartments" required />
                                                    </div>
                                                    <div class="col-6 col-lg-2">
                                                        <label class="form-label small fw-bold">Dükkan</label>
                                                        <input type="number" name="ExistingBlocks[@i].TotalShops" class="form-control" value="@b.TotalShops" required />
                                                    </div>
                                                    <div class="col-6 col-lg-2">
                                                        <label class="form-label small fw-bold">Yapım Yılı</label>
                                                        <input type="number" name="ExistingBlocks[@i].BuildingAge" class="form-control" placeholder="Örn: 1998" value="@b.BuildingAge" />
                                                    </div>
                                                    <div class="col-4 col-lg-2">
                                                        <label class="form-label small fw-bold">Bodrum Kat</label>
                                                        <input type="number" name="ExistingBlocks[@i].BasementFloors" class="form-control" value="@b.BasementFloors" required />
                                                    </div>
                                                    <div class="col-4 col-lg-1">
                                                        <label class="form-label small fw-bold">Zemin Kat</label>
                                                        <div class="form-check form-switch mt-1">
                                                            <input class="form-check-input" type="checkbox" name="ExistingBlocks[@i].HasGroundFloor" value="true" checked="@b.HasGroundFloor">
                                                        </div>
                                                    </div>
                                                    <div class="col-4 col-lg-1">
                                                        <label class="form-label small fw-bold">Çatı Katı</label>
                                                        <div class="form-check form-switch mt-1">
                                                            <input class="form-check-input" type="checkbox" name="ExistingBlocks[@i].HasRoof" value="true" checked="@b.HasRoof">
                                                        </div>
                                                    </div>
                                                </div>
                                                
                                                <div class="sub-blocks-container mt-4 pt-3 border-top border-danger border-opacity-25" style="display: none;">
                                                    <div class="row">
                                                        <div class="col-12 col-lg-4">
                                                            <label class="form-label small fw-bold text-danger sub-block-label">Kaç Kule / Blok Var?</label>
                                                            <input type="number" class="form-control form-control-sm" min="0" max="10" value="0" oninput="generateSubBlocks(this, 'ExistingBlocks', @i)" />
                                                        </div>
                                                    </div>
                                                    <div class="sub-blocks-list"></div>
                                                </div>
                                            </div>
                                        </div>'''

def replace_razor_new(match):
    return '''<h5 class="fw-bold text-primary border-bottom pb-2 mb-3"><i class="ph ph-building me-2"></i> @(i+1). Yeni Yapı</h5>
                                                <div class="row g-3">
                                                    <div class="col-md-12 col-lg-4">
                                                        <label class="form-label small fw-bold">Yeni Yapı Adı</label>
                                                        <input type="text" name="TargetBlocks[@i].BlockName" class="form-control fw-bold" value="@b.BlockName" required />
                                                    </div>
                                                    <div class="col-12 col-lg-4">
                                                        <label class="form-label small fw-bold">Temel / Baza Durumu</label>
                                                        <select name="TargetBlocks[@i].LayoutPattern" class="form-select form-select-sm mt-1" onchange="handleLayoutPatternChange(this)">
                                                            <option value="" selected="@(string.IsNullOrEmpty(b.LayoutPattern))">Mimari Tarz (Seçiniz)</option>
                                                            <option value="Tekil Yapı" selected="@(b.LayoutPattern == "Tekil Yapı")">Tekil Yapı (Standart)</option>
                                                            <option value="Bitişik Nizam" selected="@(b.LayoutPattern == "Bitişik Nizam")">Bitişik Nizam (Tevhit)</option>
                                                            <option value="Ortak Baza" selected="@(b.LayoutPattern == "Ortak Baza")">Ortak Baza (Kule Altı)</option>
                                                            <option value="Kule" selected="@(b.LayoutPattern == "Kule")">Kule (Baza Üstü)</option>
                                                        </select>
                                                    </div>
                                                    <div class="col-12 col-lg-4">
                                                        <label class="form-label small fw-bold">Taban (m²)</label>
                                                        <input type="number" name="TargetBlocks[@i].BaseArea" class="form-control" placeholder="Örn: 200" min="1" value="@b.BaseArea" required />
                                                    </div>
                                                </div>
                                                
                                                <div class="row g-3 mt-3 tech-details" style="display: none; padding-top: 15px; border-top: 1px dashed #ccc;">
                                                    <div class="col-6 col-lg-3">
                                                        <label class="form-label small fw-bold">Kat Sayısı</label>
                                                        <input type="number" name="TargetBlocks[@i].TotalFloors" class="form-control" min="1" value="@b.TotalFloors" required />
                                                    </div>
                                                    <div class="col-6 col-lg-2">
                                                        <label class="form-label small fw-bold">Daire</label>
                                                        <input type="number" name="TargetBlocks[@i].TotalApartments" class="form-control" value="@b.TotalApartments" required />
                                                    </div>
                                                    <div class="col-6 col-lg-2">
                                                        <label class="form-label small fw-bold">Dükkan</label>
                                                        <input type="number" name="TargetBlocks[@i].TotalShops" class="form-control" value="@b.TotalShops" required />
                                                    </div>
                                                    <div class="col-6 col-lg-3">
                                                        <label class="form-label small fw-bold">Bodrum Kat</label>
                                                        <input type="number" name="TargetBlocks[@i].BasementFloors" class="form-control" value="@b.BasementFloors" required />
                                                    </div>
                                                    <div class="col-6 col-lg-1">
                                                        <label class="form-label small fw-bold">Zemin Kat</label>
                                                        <div class="form-check form-switch mt-1">
                                                            <input class="form-check-input" type="checkbox" name="TargetBlocks[@i].HasGroundFloor" value="true" checked="@b.HasGroundFloor">
                                                        </div>
                                                    </div>
                                                    <div class="col-6 col-lg-1">
                                                        <label class="form-label small fw-bold">Çatı Katı</label>
                                                        <div class="form-check form-switch mt-1">
                                                            <input class="form-check-input" type="checkbox" name="TargetBlocks[@i].HasRoof" value="true" checked="@b.HasRoof">
                                                        </div>
                                                    </div>
                                                </div>
                                                
                                                <div class="sub-blocks-container mt-4 pt-3 border-top border-primary border-opacity-25" style="display: none;">
                                                    <div class="row">
                                                        <div class="col-12 col-lg-4">
                                                            <label class="form-label small fw-bold text-primary sub-block-label">Kaç Kule / Blok Var?</label>
                                                            <input type="number" class="form-control form-control-sm" min="0" max="10" value="0" oninput="generateSubBlocks(this, 'TargetBlocks', @i)" />
                                                        </div>
                                                    </div>
                                                    <div class="sub-blocks-list"></div>
                                                </div>
                                            </div>
                                        </div>'''

def replace_js_old(match):
    return '''<h5 class="fw-bold text-danger border-bottom pb-2 mb-3"><i class="ph ph-building me-2"></i> ${i+1}. Eski Yapı</h5>
                              <div class="row g-3">
                                  <div class="col-md-12 col-lg-4">
                                      <label class="form-label small fw-bold">Eski Yapı Adı</label>
                                      <input type="text" name="ExistingBlocks[${i}].BlockName" class="form-control fw-bold" value="${defaultName}" required />
                                  </div>
                                  <div class="col-12 col-lg-4">
                                      <label class="form-label small fw-bold">Temel / Baza Durumu</label>
                                      <select name="ExistingBlocks[${i}].LayoutPattern" class="form-select form-select-sm mt-1" onchange="handleLayoutPatternChange(this)">
                                          <option value="" selected>Mimari Tarz (Seçiniz)</option>
                                          <option value="Tekil Yapı">Tekil Yapı (Standart)</option>
                                          <option value="Bitişik Nizam">Bitişik Nizam (Tevhit)</option>
                                          <option value="Ortak Baza">Ortak Baza (Kule Altı)</option>
                                          <option value="Kule">Kule (Baza Üstü)</option>
                                      </select>
                                  </div>
                                  <div class="col-12 col-lg-4">
                                      <label class="form-label small fw-bold">Taban (m²)</label>
                                      <input type="number" name="ExistingBlocks[${i}].BaseArea" class="form-control" placeholder="Örn: 200" min="1" required />
                                  </div>
                              </div>
                              
                              <div class="row g-3 mt-3 tech-details" style="display: none; padding-top: 15px; border-top: 1px dashed #ccc;">
                                  <div class="col-6 col-lg-2">
                                      <label class="form-label small fw-bold">Kat Sayısı</label>
                                      <input type="number" name="ExistingBlocks[${i}].TotalFloors" class="form-control" min="1" required />
                                  </div>
                                    <div class="col-6 col-lg-2">
                                        <label class="form-label small fw-bold">Yapım Yılı</label>
                                        <input type="number" name="ExistingBlocks[${i}].BuildingAge" class="form-control" placeholder="Örn: 1998" />
                                    </div>
                                  <div class="col-6 col-lg-2">
                                      <label class="form-label small fw-bold">Daire</label>
                                      <input type="number" name="ExistingBlocks[${i}].TotalApartments" class="form-control" required />
                                  </div>
                                  <div class="col-6 col-lg-2">
                                      <label class="form-label small fw-bold">Dükkan</label>
                                      <input type="number" name="ExistingBlocks[${i}].TotalShops" class="form-control" value="0" required />
                                  </div>
                                  <div class="col-6 col-lg-2">
                                      <label class="form-label small fw-bold">Bodrum Kat</label>
                                      <input type="number" name="ExistingBlocks[${i}].BasementFloors" class="form-control" value="0" required />
                                  </div>
                                  <div class="col-4 col-lg-1">
                                      <label class="form-label small fw-bold">Zemin Kat</label>
                                      <div class="form-check form-switch mt-1">
                                          <input class="form-check-input" type="checkbox" name="ExistingBlocks[${i}].HasGroundFloor" value="true" checked>
                                      </div>
                                  </div>
                                  <div class="col-4 col-lg-1">
                                      <label class="form-label small fw-bold">Çatı Katı</label>
                                      <div class="form-check form-switch mt-1">
                                          <input class="form-check-input" type="checkbox" name="ExistingBlocks[${i}].HasRoof" value="true" checked>
                                      </div>
                                  </div>
                              </div>
                              
                              <div class="sub-blocks-container mt-4 pt-3 border-top border-danger border-opacity-25" style="display: none;">
                                  <div class="row">
                                      <div class="col-12 col-lg-4">
                                          <label class="form-label small fw-bold text-danger sub-block-label">Kaç Kule / Blok Var?</label>
                                          <input type="number" class="form-control form-control-sm" min="0" max="10" value="0" oninput="generateSubBlocks(this, 'ExistingBlocks', ${i})" />
                                      </div>
                                  </div>
                                  <div class="sub-blocks-list"></div>
                              </div>
                          </div>
                      </div>`;'''

def replace_js_new(match):
    return '''<h5 class="fw-bold text-primary border-bottom pb-2 mb-3"><i class="ph ph-building me-2"></i> ${i+1}. Yeni Yapı</h5>
                              <div class="row g-3">
                                  <div class="col-md-12 col-lg-4">
                                      <label class="form-label small fw-bold">Yeni Yapı Adı</label>
                                      <input type="text" name="TargetBlocks[${i}].BlockName" class="form-control fw-bold" value="${defaultName}" required />
                                  </div>
                                  <div class="col-12 col-lg-4">
                                      <label class="form-label small fw-bold">Temel / Baza Durumu</label>
                                      <select name="TargetBlocks[${i}].LayoutPattern" class="form-select form-select-sm mt-1" onchange="handleLayoutPatternChange(this)">
                                          <option value="" selected>Mimari Tarz (Seçiniz)</option>
                                          <option value="Tekil Yapı">Tekil Yapı (Standart)</option>
                                          <option value="Bitişik Nizam">Bitişik Nizam (Tevhit)</option>
                                          <option value="Ortak Baza">Ortak Baza (Kule Altı)</option>
                                          <option value="Kule">Kule (Baza Üstü)</option>
                                      </select>
                                  </div>
                                  <div class="col-12 col-lg-4">
                                      <label class="form-label small fw-bold">Taban (m²)</label>
                                      <input type="number" name="TargetBlocks[${i}].BaseArea" class="form-control" placeholder="Örn: 200" min="1" required />
                                  </div>
                              </div>
                              
                              <div class="row g-3 mt-3 tech-details" style="display: none; padding-top: 15px; border-top: 1px dashed #ccc;">
                                  <div class="col-6 col-lg-3">
                                      <label class="form-label small fw-bold">Kat Sayısı</label>
                                      <input type="number" name="TargetBlocks[${i}].TotalFloors" class="form-control" min="1" required />
                                  </div>
                                  <div class="col-6 col-lg-2">
                                      <label class="form-label small fw-bold">Daire</label>
                                      <input type="number" name="TargetBlocks[${i}].TotalApartments" class="form-control" required />
                                  </div>
                                  <div class="col-6 col-lg-2">
                                      <label class="form-label small fw-bold">Dükkan</label>
                                      <input type="number" name="TargetBlocks[${i}].TotalShops" class="form-control" value="0" required />
                                  </div>
                                  <div class="col-6 col-lg-3">
                                      <label class="form-label small fw-bold">Bodrum Kat</label>
                                      <input type="number" name="TargetBlocks[${i}].BasementFloors" class="form-control" value="0" required />
                                  </div>
                                  <div class="col-6 col-lg-1">
                                      <label class="form-label small fw-bold">Zemin Kat</label>
                                      <div class="form-check form-switch mt-1">
                                          <input class="form-check-input" type="checkbox" name="TargetBlocks[${i}].HasGroundFloor" value="true" checked>
                                      </div>
                                  </div>
                                  <div class="col-6 col-lg-1">
                                      <label class="form-label small fw-bold">Çatı Katı</label>
                                      <div class="form-check form-switch mt-1">
                                          <input class="form-check-input" type="checkbox" name="TargetBlocks[${i}].HasRoof" value="true" checked>
                                      </div>
                                  </div>
                              </div>
                              
                              <div class="sub-blocks-container mt-4 pt-3 border-top border-primary border-opacity-25" style="display: none;">
                                  <div class="row">
                                      <div class="col-12 col-lg-4">
                                          <label class="form-label small fw-bold text-primary sub-block-label">Kaç Kule / Blok Var?</label>
                                          <input type="number" class="form-control form-control-sm" min="0" max="10" value="0" oninput="generateSubBlocks(this, 'TargetBlocks', ${i})" />
                                      </div>
                                  </div>
                                  <div class="sub-blocks-list"></div>
                              </div>
                          </div>
                      </div>`;'''

# Using regex matching the OLD code structure with "Blok / Yapı" to fix it properly!
content = re.sub(r'<h5 class="fw-bold text-danger mb-3"><i class="ph ph-building me-2"></i> @\(i\+1\)\. Eski Blok / Yapı</h5>.*?(?=</div>\s*</div>\s*</div>\s*i\+\+;)', replace_razor_old, content, flags=re.DOTALL)
content = re.sub(r'<h5 class="fw-bold text-primary mb-3"><i class="ph ph-building me-2"></i> @\(i\+1\)\. Yeni Blok / Yapı</h5>.*?(?=</div>\s*</div>\s*</div>\s*i\+\+;)', replace_razor_new, content, flags=re.DOTALL)
content = re.sub(r'<h5 class="fw-bold text-danger mb-3"><i class="ph ph-building me-2"></i> \$\{i\+1\}\. Eski Blok / Yapı</h5>.*?(?=</div>\s*</div>\s*</div>`;)', replace_js_old, content, flags=re.DOTALL)
content = re.sub(r'<h5 class="fw-bold text-primary mb-3"><i class="ph ph-building me-2"></i> \$\{i\+1\}\. Yeni Blok / Yapı</h5>.*?(?=</div>\s*</div>\s*</div>`;)', replace_js_new, content, flags=re.DOTALL)

with open('GMK360.Web/Views/ConstructionProject/Create.cshtml', 'w', encoding='utf-8') as f:
    f.write(content)
print("Complete UI Restoration applied!")
