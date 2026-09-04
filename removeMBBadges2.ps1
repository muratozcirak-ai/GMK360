$path = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml"
$text = [System.IO.File]::ReadAllText($path)

# Remove "Birim" badge
$text = [System.Text.RegularExpressions.Regex]::Replace($text, '<span class="badge bg-secondary text-dark border">@floor.Units.Count\(\) Birim<\/span>', '')
$text = [System.Text.RegularExpressions.Regex]::Replace($text, '<span class="badge bg-secondary">@\(floorGroup.Count\(\)\) Birim<\/span>', '')
$text = [System.Text.RegularExpressions.Regex]::Replace($text, '<span class="badge bg-primary rounded-pill px-3 py-2 ms-3 fs-6">Toplam @totalUnits Adet<\/span>', '')

[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
