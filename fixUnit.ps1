$text = [System.IO.File]::ReadAllText("c:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\BuildingUnit.cs")
$search = "public virtual System.Collections.Generic.ICollection<UnitDebt> Debts { get; set; } = new System.Collections.Generic.List<UnitDebt>();"
$replace = $search + "`r`n        public virtual System.Collections.Generic.ICollection<UnitSpace> Spaces { get; set; } = new System.Collections.Generic.List<UnitSpace>();"
$text = $text.Replace($search, $replace)
[System.IO.File]::WriteAllText("c:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\BuildingUnit.cs", $text, [System.Text.Encoding]::UTF8)
