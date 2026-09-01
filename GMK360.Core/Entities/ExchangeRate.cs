using System;

namespace GMK360.Core.Entities
{
    public class ExchangeRate : BaseEntity
    {
        public DateTime Date { get; set; }
        public decimal UsdRate { get; set; }
        public decimal EurRate { get; set; }
    }
}
