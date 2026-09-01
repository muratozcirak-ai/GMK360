namespace GMK360.Core.Entities
{
    public class City : BaseEntity
    {
        public int CountryId { get; set; }
        public Country Country { get; set; }

        public string Name { get; set; }
        public string PlateCode { get; set; }
    }
}
