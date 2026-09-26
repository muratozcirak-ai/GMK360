namespace GMK360.Core.Entities
{
    public class InstitutionContact
    {
        public int Id { get; set; }
        public int AgencyId { get; set; }
        public Agency Agency { get; set; }

        public int InstitutionRecordId { get; set; }
        public InstitutionRecord Institution { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string PhoneNumber { get; set; }
        public string Notes { get; set; }
    }
}
