namespace GMK360.Core.Entities.B2b
{
    public class B2bCompanyCategory : BaseEntity
    {
        public int B2bCompanyId { get; set; }
        public B2bCompany B2bCompany { get; set; }

        public int DefinitionValueId { get; set; }
        public DefinitionValue DefinitionValue { get; set; }
    }
}
