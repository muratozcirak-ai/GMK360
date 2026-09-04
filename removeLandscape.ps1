$path = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Details.cshtml"
$text = [System.IO.File]::ReadAllText($path)

$pattern = '(?s)<div class="col-sm-6">\s*<div class="d-flex align-items-center">\s*<i class="bi bi-tree text-success fs-4 me-3"></i>\s*<div>\s*<div class="text-muted small">Peyzaj / Yeşil Alan</div>\s*<div class="fw-bold text-dark">@\(Model\.LandscapeArea\.HasValue \? Model\.LandscapeArea\.Value \+ " m²" : "Belirtilmedi"\)</div>\s*</div>\s*</div>\s*</div>'

$text = $text -replace $pattern, ''

[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
