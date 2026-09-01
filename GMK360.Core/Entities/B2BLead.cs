namespace GMK360.Core.Entities
{
    public class B2BLead : BaseEntity
    {
        public string AgencyName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        
        // Bu kayıt arandı mı? (Satış CRM'i için)
        public bool IsContacted { get; set; }
        public string AdminNotes { get; set; }
    }
}
