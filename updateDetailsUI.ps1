$path = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Details.cshtml"
$text = [System.IO.File]::ReadAllText($path)

$amenitiesSection = @"
                    </div>
                    
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
                                                <i class="bi @(amenity.IconClass ?? "bi-check-circle") text-primary fs-4 me-3"></i>
                                                <div>
                                                    <div class="text-muted small">@amenity.Name</div>
                                                    <div class="fw-bold text-dark">
                                                        @(amenity.AreaSquareMeters.HasValue ? amenity.AreaSquareMeters.Value + " m²" : "Mevcut")
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    }
                                }
                            </div>
                        </div>
                    }
                </div>
"@

$text = $text.Replace("                    </div>`r`n                </div>", $amenitiesSection)

[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
