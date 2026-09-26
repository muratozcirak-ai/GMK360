using System.Collections.Generic;

namespace GMK360.Core.Entities
{
    public class ModuleDocumentRule : BaseEntity
    {
        public string TargetModule { get; set; } = string.Empty; 
        public string Stage { get; set; } = string.Empty;        
        public int DisplayOrder { get; set; } = 0;
        
        public int SystemLegalDocumentTemplateId { get; set; }
        public SystemLegalDocumentTemplate SystemLegalDocumentTemplate { get; set; }
        
        public ICollection<ModuleDocumentRulePrerequisite> Prerequisites { get; set; } = new List<ModuleDocumentRulePrerequisite>();
    }
}
