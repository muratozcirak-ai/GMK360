$path = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Details.cshtml"
$text = [System.IO.File]::ReadAllText($path)

$text = $text.Replace("amenity.AreaSquareMeters", "amenity.SquareMeters")
$text = $text.Replace("amenity.IconClass ?? ""bi-check-circle""", """bi-check-circle""")

[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
