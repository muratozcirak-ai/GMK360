using System.Collections.Generic;

namespace GMK360.Web.Models
{
    public class SearchFilterDto
    {
        public string Keyword { get; set; } // Omnibox Search (e.g. "Ataşehir 3+1 site içi")
        public int TransactionTypeId { get; set; } // 1: Satılık, 2: Kiralık vs.
        
        // Multi-select Fields
        public List<int> CityIds { get; set; } = new List<int>();
        public List<int> DistrictIds { get; set; } = new List<int>();
        public List<int> NeighborhoodIds { get; set; } = new List<int>();
        public List<int> CategoryIds { get; set; } = new List<int>(); // SubType Ids
        
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        
        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}
