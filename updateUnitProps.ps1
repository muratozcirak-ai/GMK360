$path = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs"
$text = [System.IO.File]::ReadAllText($path)

$oldSig = "public async Task<IActionResult> UpdateUnitProperties(int Id, int BuildingId, string DoorNumber, string RoomLayout, string OwnerName, string OwnerPhone)"
$newSig = "public async Task<IActionResult> UpdateUnitProperties(int Id, int BuildingId, string DoorNumber, string RoomLayout, string OwnerName, string OwnerPhone, double? GrossSquareMeters, double? NetSquareMeters, string FacadeDirection)"

$text = $text.Replace($oldSig, $newSig)
$text = $text.Replace("unit.OwnerPhone = OwnerPhone;", "unit.OwnerPhone = OwnerPhone;`r`n                unit.GrossSquareMeters = GrossSquareMeters;`r`n                unit.NetSquareMeters = NetSquareMeters;`r`n                unit.FacadeDirection = FacadeDirection;")

[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
