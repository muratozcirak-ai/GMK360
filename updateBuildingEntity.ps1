$content = Get-Content -Raw "C:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\Building.cs"

$newProps = @"
        public int? ConstructionProjectId { get; set; }
        public virtual GMK360.Core.Entities.Construction.ConstructionProject ConstructionProject { get; set; }
        
        // Podyum ve Kule Hiyerarşisi
        public int? ParentBuildingId { get; set; }
        public virtual Building ParentBuilding { get; set; }
        public virtual ICollection<Building> ChildBuildings { get; set; } = new List<Building>();
"@

$content = $content.Replace("public int? ConstructionProjectId { get; set; }`r`n        public virtual GMK360.Core.Entities.Construction.ConstructionProject ConstructionProject { get; set; }", $newProps)

$content | Set-Content "C:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\Building.cs" -Encoding UTF8
Write-Output "Building.cs updated"
