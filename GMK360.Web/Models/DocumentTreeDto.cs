using GMK360.Core.Entities;

namespace GMK360.Web.Models
{
    public class DocumentTreeDto
    {
        public SystemLegalDocumentTemplate Document { get; set; }
        public int Level { get; set; }
        public bool IsRoot { get; set; }
    }
}
