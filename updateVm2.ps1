$content = Get-Content -Raw "GMK360.Web\Models\CreateProjectWizardViewModel.cs"

$newProps = @"
        public int? BasementFloors { get; set; }
        public int? TotalApartments { get; set; }
        public int? TotalShops { get; set; }
        public bool HasRoof { get; set; }
        public bool HasGroundFloor { get; set; } = true;

        // Podyum ve Kule
        public string? StructureType { get; set; } // "independent", "podium", "tower"
        public int? ParentIndex { get; set; } // Kule ise hangi bazaya ait?
"@

$content = $content.Replace("public int? BasementFloors { get; set; }`r`n        public int? TotalApartments { get; set; }`r`n        public int? TotalShops { get; set; }`r`n        public bool HasRoof { get; set; }`r`n        public bool HasGroundFloor { get; set; } = true;", $newProps)

$content | Set-Content "GMK360.Web\Models\CreateProjectWizardViewModel.cs" -Encoding UTF8
Write-Output "ViewModel updated"
