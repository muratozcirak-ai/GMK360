$path = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs"
$text = [System.IO.File]::ReadAllText($path)

$oldSig = "public async Task<IActionResult> UpdateBlockDetails(int Id, string BlockName, double? BaseArea, string FacadeDirection, string TechnicalFeatures, string Description)"
$newSig = "public async Task<IActionResult> UpdateBlockDetails(int Id, string BlockName, double? BaseArea, string FacadeDirection, string TechnicalFeatures, string Description, string InsulationType, int? ElevatorCount, string ParkingType)"

$text = $text.Replace($oldSig, $newSig)
$text = $text.Replace("block.Description = Description;", "block.Description = Description;`r`n                block.InsulationType = InsulationType;`r`n                block.ElevatorCount = ElevatorCount;`r`n                block.ParkingType = ParkingType;")

[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
