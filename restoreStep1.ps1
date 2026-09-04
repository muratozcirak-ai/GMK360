$content = Get-Content -Raw "GMK360.Web\Views\ConstructionProject\Create.cshtml"

$oldHtml = @"
                            <div class="col-md-12">
                                <label class="form-label fw-bold">Mimari Çizim / Proje Görseli (URL)</label>
                                <input asp-for="CoverImageFile" type="file" class="form-control rounded-3" accept="image/jpeg,image/png,application/pdf" />
                                <div class="form-text text-muted">Mimari resim veya 3D render görselini bilgisayarınızdan seçin (JPG/PNG).</div>
                            </div>
"@

$newHtml = @"
                            <div class="col-md-12">
                                <div class="row g-3">
                                    <div class="col-md-6">
                                        <label class="form-label fw-bold">Toplam Arazi Alanı (m²)</label>
                                        <div class="input-group">
                                            <input type="number" asp-for="TotalLandArea" class="form-control rounded-start-3" placeholder="Örn: 2500" min="1" required />
                                            <span class="input-group-text rounded-end-3">m²</span>
                                        </div>
                                    </div>
                                    <div class="col-md-6">
                                    </div>
                                    <div class="col-md-6">
                                        <label class="form-label fw-bold">Mevcut Durum Görseli (İlk Hali)</label>
                                        <input asp-for="CurrentStateImageFile" type="file" class="form-control rounded-3" accept="image/jpeg,image/png,application/pdf" />
                                        <div class="form-text text-muted">Şantiyenin/arazinin şu anki hali (JPG/PNG).</div>
                                    </div>
                                    <div class="col-md-6">
                                        <label class="form-label fw-bold">Proje Görseli (Geleceği Hali)</label>
                                        <input asp-for="CoverImageFile" type="file" class="form-control rounded-3" accept="image/jpeg,image/png,application/pdf" />
                                        <div class="form-text text-muted">Mimari 3D render görselini seçin (JPG/PNG).</div>
                                    </div>
                                </div>
                            </div>
"@

$content = $content.Replace($oldHtml, $newHtml)

$content | Set-Content "GMK360.Web\Views\ConstructionProject\Create.cshtml" -Encoding UTF8
Write-Output "Step 1 UI restored"
