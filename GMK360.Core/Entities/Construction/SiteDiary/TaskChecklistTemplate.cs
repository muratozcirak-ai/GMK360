using System;

namespace GMK360.Core.Entities.Construction.SiteDiary
{
    // 3. PASÝF KONTROLLER (Checklist)
    public class TaskChecklistTemplate : BaseEntity
    {
        public int TaskTemplateId { get; set; }
        
        public string KontrolMaddesi { get; set; } // Örn: ""Demir paspaylarý kondu mu?""
        public bool FotografZorunluMu { get; set; } // true ise resim yüklemeden onay veremez
        public bool IseEngelMi { get; set; } // true ise bu madde onaylanmadan ana iþ yapýlamaz

        public virtual TaskTemplate TaskTemplate { get; set; }
    }
}
