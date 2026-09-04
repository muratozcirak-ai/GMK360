$path = "c:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\BuildingUnit.cs"
$text = [System.IO.File]::ReadAllText($path)
$insert = @"
        public int? UnitTemplateId { get; set; }
        public virtual GMK360.Core.Entities.Construction.UnitTemplate UnitTemplate { get; set; }

        public virtual System.Collections.Generic.ICollection<GMK360.Core.Entities.Construction.UnitSpace> Spaces { get; set; } = new System.Collections.Generic.List<GMK360.Core.Entities.Construction.UnitSpace>();
"@

$text = $text.Replace("public virtual System.Collections.Generic.ICollection<UnitDebt> Debts", $insert + "`r`n        public virtual System.Collections.Generic.ICollection<UnitDebt> Debts")
[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
