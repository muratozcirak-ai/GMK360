using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using GMK360.Core.Entities;
using GMK360.Core.Interfaces;

namespace GMK360.Core.Services
{
    // EmlakJet veya HepsiEmlak formatına uygun basit bir XML Çıktı Modeli
    [XmlRoot("Ilanlar")]
    public class XmlPropertyFeed
    {
        [XmlElement("Ilan")]
        public List<XmlPropertyItem> Properties { get; set; } = new List<XmlPropertyItem>();
    }

    public class XmlPropertyItem
    {
        [XmlElement("IlanNo")]
        public int Id { get; set; }
        
        [XmlElement("Baslik")]
        public string Title { get; set; }
        
        [XmlElement("Fiyat")]
        public decimal Price { get; set; }
        
        [XmlElement("Aciklama")]
        public string Description { get; set; }
    }

    public class XmlExportService
    {
        public async Task<string> GenerateXmlFeedAsync(List<Property> properties)
        {
            var feed = new XmlPropertyFeed();
            foreach (var prop in properties)
            {
                feed.Properties.Add(new XmlPropertyItem
                {
                    Id = prop.Id,
                    Title = prop.Title,
                    Price = prop.Price,
                    Description = prop.Description
                });
            }

            var serializer = new XmlSerializer(typeof(XmlPropertyFeed));
            using var stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, feed);
            
            return stringWriter.ToString();
        }
    }
}
