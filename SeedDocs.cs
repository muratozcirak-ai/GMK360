using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Admin;

var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseSqlServer(@""Server=(localdb)\mssqllocaldb;Database=GMK360Db;Trusted_Connection=True;MultipleActiveResultSets=true"");

using (var db = new ApplicationDbContext(optionsBuilder.Options))
{
    var existing = db.SystemLegalDocumentTemplates.Any();
    if (!existing)
    {
        var docs = new List<SystemLegalDocumentTemplate>
        {
            new SystemLegalDocumentTemplate { Name = ""Arsa Tapusu"", IssuedBy = ""Tapu Müdürlüğü"", TargetModule = ""Construction"", IsRequired = true },
            new SystemLegalDocumentTemplate { Name = ""İmar Durumu Belgesi"", IssuedBy = ""Belediye"", TargetModule = ""Construction"", IsRequired = true },
            new SystemLegalDocumentTemplate { Name = ""Mimari Proje Onayı"", IssuedBy = ""Belediye İmar Müdürlüğü"", TargetModule = ""Construction"", IsRequired = true },
            new SystemLegalDocumentTemplate { Name = ""Zemin Etüdü Raporu"", IssuedBy = ""Özel Zemin/Jeoloji Firması"", TargetModule = ""Construction"", IsRequired = true },
            new SystemLegalDocumentTemplate { Name = ""Yapı Ruhsatı"", IssuedBy = ""Belediye"", TargetModule = ""Construction"", IsRequired = true }
        };
        db.SystemLegalDocumentTemplates.AddRange(docs);
        db.SaveChanges();
        Console.WriteLine(""Evraklar eklendi."");
    }
    else {
        Console.WriteLine(""Evraklar zaten var."");
    }
}
