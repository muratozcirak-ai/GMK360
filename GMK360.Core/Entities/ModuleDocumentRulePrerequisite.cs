namespace GMK360.Core.Entities
{
    public class ModuleDocumentRulePrerequisite : BaseEntity
    {
        public int ModuleDocumentRuleId { get; set; }
        public ModuleDocumentRule ModuleDocumentRule { get; set; }

        public int PrerequisiteTemplateId { get; set; }
        public SystemLegalDocumentTemplate PrerequisiteTemplate { get; set; }
    }
}
