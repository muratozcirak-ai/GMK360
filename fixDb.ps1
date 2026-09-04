$text = [System.IO.File]::ReadAllText("c:\Users\murat\source\repos\GMK360\GMK360.Data\Contexts\ApplicationDbContext.cs")
$search = "public DbSet<BuildingUnit> BuildingUnits { get; set; }"
$replace = $search + "`r`n        public DbSet<UnitSpace> UnitSpaces { get; set; }"
$text = $text.Replace($search, $replace)
[System.IO.File]::WriteAllText("c:\Users\murat\source\repos\GMK360\GMK360.Data\Contexts\ApplicationDbContext.cs", $text, [System.Text.Encoding]::UTF8)
