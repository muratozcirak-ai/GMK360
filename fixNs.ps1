$path = "c:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\BuildingUnit.cs"
$text = [System.IO.File]::ReadAllText($path)
$text = $text.Replace("GMK360.Core.Entities.Construction.UnitTemplate", "GMK360.Core.Entities.UnitTemplate")
$text = $text.Replace("GMK360.Core.Entities.Construction.UnitSpace", "GMK360.Core.Entities.UnitSpace")
[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
