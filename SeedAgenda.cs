using System;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Construction;
using Microsoft.EntityFrameworkCore;

class Program
{
    static async Task Main()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(""Server=(localdb)\\mssqllocaldb;Database=GMK360DB;Trusted_Connection=True;MultipleActiveResultSets=true"")
            .Options;
            
        using var context = new ApplicationDbContext(options);
        var agenda = await context.AgendaRecords.FirstOrDefaultAsync();
        
        if (agenda != null && !context.AgendaItems.Any())
        {
            context.AgendaItems.Add(new AgendaItem
            {
                AgendaRecordId = agenda.Id,
                OrderNo = 1,
                TopicTitle = ""A Blok Temel Betonu Fiyatları"",
                PresentationText = ""A Blok temel betonu için 3 farklı hazır beton firmasından fiyat alınmıştır.\nEn uygun teklif 150.000 TL olarak belirlendi.\nBuna göre döküm programı oluşturulacaktır."",
                ImageUrl = ""https://images.unsplash.com/photo-1541888081622-12a832f05a96?auto=format&fit=crop&q=80&w=800"",
                LiveMeetingNotes = """"
            });
            
            context.AgendaItems.Add(new AgendaItem
            {
                AgendaRecordId = agenda.Id,
                OrderNo = 2,
                TopicTitle = ""Şantiye Güvenlik Denetimi Eksikleri"",
                PresentationText = ""İş güvenliği uzmanı tarafından hazırlanan 10 Ekim tarihli raporda, iskele bağlantılarında 3 noktada zayıflık tespit edilmiştir.\nAcil müdahale edilmesi gerekmektedir."",
                ImageUrl = ""https://images.unsplash.com/photo-1503387762-592deb58ef4e?auto=format&fit=crop&q=80&w=800"",
                LiveMeetingNotes = """"
            });
            
            await context.SaveChangesAsync();
            Console.WriteLine(""Seeded AgendaItems successfully!"");
        }
    }
}
