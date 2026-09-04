$path = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs"
$text = [System.IO.File]::ReadAllText($path)

$text = $text.Replace(".Include(c => c.Phases)", ".Include(c => c.Phases)`r`n                .Include(c => c.Amenities)")

[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
