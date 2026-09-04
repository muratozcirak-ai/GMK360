$path = "c:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\Building.cs"
$text = [System.IO.File]::ReadAllText($path)
$insert = @"
        public string InsulationType { get; set; } // Mantolama/Yalıtım
        public int? ElevatorCount { get; set; } // Asansör Sayısı
        public string ParkingType { get; set; } // Otopark (Açık, Kapalı, Yok)
"@

$text = $text.Replace("public string TechnicalFeatures { get; set; }", "public string TechnicalFeatures { get; set; }`r`n" + $insert)
[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
