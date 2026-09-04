$path = "c:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\BuildingUnit.cs"
$text = [System.IO.File]::ReadAllText($path)
$insert = @"
        public double? GrossSquareMeters { get; set; }
        public double? NetSquareMeters { get; set; }
        public string FacadeDirection { get; set; }
"@

$text = $text.Replace("public string UnitNumber { get => DoorNumber; set => DoorNumber = value; }", "public string UnitNumber { get => DoorNumber; set => DoorNumber = value; }`r`n" + $insert)
[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
