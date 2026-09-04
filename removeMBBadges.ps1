$path = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml"
$text = [System.IO.File]::ReadAllText($path)

# Remove the "Toplam X Adet" badge from the card header
$patternHeaderBadge = '<span class="badge bg-primary rounded-pill px-3 py-2 ms-3 fs-6">Toplam @totalUnits Adet<\/span>'
$text = [System.Text.RegularExpressions.Regex]::Replace($text, $patternHeaderBadge, "")

# Remove the "2 Birim" badge from accordion buttons
$patternUnitBadge = '<div class="ms-3 me-3 flex-shrink-0">\s*<span class="badge bg-secondary text-dark border">@floor\.Units\.Count\(\) Birim<\/span>\s*<\/div>'
$text = [System.Text.RegularExpressions.Regex]::Replace($text, $patternUnitBadge, "")

# Also, the user says "ana sayfada etiket kapatıyor". They uploaded an image showing:
# "Toplam 22 Adet" badge overlapping "Kat Ağacı ve Bağımsız Bölümler"
# "2 Birim" badge overlapping the floor names
# I'll just remove them entirely as requested "2 birim yazmasına gerek yok".

# Let's also simplify the HTML of accordion buttons in ManageBlock to prevent ANY flex issues.
# Right now it's:
# <span class="text-truncate me-auto fw-bold"><i class="bi bi-building-up me-2 text-primary"></i> @floorName</span>
# I will just make it standard text.
$text = $text.Replace('<div class="d-flex w-100 align-items-center pe-5">', '<div>')
$text = $text.Replace('<div class="text-truncate me-auto fw-bold">', '<span>')
$text = $text.Replace('</div>
                                            <div class="flex-shrink-0 text-end">', ' ')
$text = $text.Replace('</div>
                                         </div>', '</span>')

[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
