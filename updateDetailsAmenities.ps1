$path = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Details.cshtml"
$text = [System.IO.File]::ReadAllText($path)

$oldBlock = @"
                    @if (Model.TotalLandArea.HasValue || Model.LandscapeArea.HasValue || (Model.Amenities != null && Model.Amenities.Any()))
                    {
                        <div class="mt-4 pt-3 border-top">
                            <div class="row g-3">
                                @if (Model.TotalLandArea.HasValue)
                                {
                                    <div class="col-sm-6">
                                        <div class="d-flex align-items-center">
                                            <i class="bi bi-aspect-ratio text-success fs-4 me-3"></i>
                                            <div>
                                                <div class="text-muted small">Toplam Arazi Alanı</div>
                                                <div class="fw-bold text-dark">@Model.TotalLandArea m²</div>
                                            </div>
                                        </div>
                                    </div>
                                }
                                @if (Model.LandscapeArea.HasValue)
                                {
                                    <div class="col-sm-6">
                                        <div class="d-flex align-items-center">
                                            <i class="bi bi-tree text-success fs-4 me-3"></i>
                                            <div>
                                                <div class="text-muted small">Peyzaj / Yeşil Alan</div>
                                                <div class="fw-bold text-dark">@Model.LandscapeArea m²</div>
                                            </div>
                                        </div>
                                    </div>
                                }
                                
                                @if (Model.Amenities != null)
                                {
                                    foreach(var amenity in Model.Amenities)
                                    {
                                        <div class="col-sm-6">
                                            <div class="d-flex align-items-center">
                                                <i class="bi bi-check-circle text-primary fs-4 me-3"></i>
                                                <div>
                                                    <div class="text-muted small">@amenity.Name</div>
                                                    <div class="fw-bold text-dark">
                                                        @(amenity.SquareMeters.HasValue ? amenity.SquareMeters.Value + " m²" : "Mevcut")
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    }
                                }
                            </div>
                        </div>
                    }
"@

$newBlock = @"
                        <div class="mt-4 pt-3 border-top position-relative">
                            
                            <div class="d-flex justify-content-between align-items-center mb-3">
                                <h6 class="fw-bold mb-0 text-muted">Arazi ve Dış Alanlar</h6>
                                <a asp-action="Amenities" asp-route-projectId="@Model.Id" class="btn btn-sm btn-outline-primary rounded-pill"><i class="bi bi-pencil me-1"></i> Yönet</a>
                            </div>

                            <div class="row g-3">
                                <div class="col-sm-6">
                                    <div class="d-flex align-items-center">
                                        <i class="bi bi-aspect-ratio text-success fs-4 me-3"></i>
                                        <div>
                                            <div class="text-muted small">Toplam Arazi Alanı</div>
                                            <div class="fw-bold text-dark">@(Model.TotalLandArea.HasValue ? Model.TotalLandArea.Value + " m²" : "Belirtilmedi")</div>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-sm-6">
                                    <div class="d-flex align-items-center">
                                        <i class="bi bi-tree text-success fs-4 me-3"></i>
                                        <div>
                                            <div class="text-muted small">Peyzaj / Yeşil Alan</div>
                                            <div class="fw-bold text-dark">@(Model.LandscapeArea.HasValue ? Model.LandscapeArea.Value + " m²" : "Belirtilmedi")</div>
                                        </div>
                                    </div>
                                </div>
                                
                                @if (Model.Amenities != null && Model.Amenities.Any())
                                {
                                    foreach(var amenity in Model.Amenities)
                                    {
                                        <div class="col-sm-6">
                                            <div class="d-flex align-items-center">
                                                <i class="bi bi-check-circle text-primary fs-4 me-3"></i>
                                                <div>
                                                    <div class="text-muted small">@amenity.Name</div>
                                                    <div class="fw-bold text-dark">
                                                        @(amenity.SquareMeters.HasValue ? amenity.SquareMeters.Value + " m²" : "Mevcut")
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    }
                                }
                                else
                                {
                                    <div class="col-12 mt-3">
                                        <div class="text-muted small fst-italic"><i class="bi bi-info-circle me-1"></i>Kamelya, açık otopark, çocuk parkı gibi dış alan donatıları henüz eklenmedi.</div>
                                    </div>
                                }
                            </div>
                        </div>
"@

$text = $text.Replace($oldBlock, $newBlock)

# Wait, check if there are duplicate blocks
$text = $text -replace '(?s)@if \(Model\.TotalLandArea\.HasValue \|\| Model\.LandscapeArea\.HasValue(.*?)Mevcut"\)\s*</div>\s*</div>\s*</div>\s*</div>\s*}\s*}\s*</div>\s*</div>\s*}', ''

[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
