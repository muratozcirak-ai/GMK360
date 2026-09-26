using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using GMK360.Infrastructure.Data;

class Program
{
    static void Main()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(@""Server=(localdb)\mssqllocaldb;Database=GMK360Db;Trusted_Connection=True;MultipleActiveResultSets=true"");
        using (var context = new ApplicationDbContext(optionsBuilder.Options))
        {
            var blocks = context.Buildings.Where(b => b.ConstructionProjectId == 14).ToList();
            context.Buildings.RemoveRange(blocks);
            context.SaveChanges();
            Console.WriteLine($""Deleted {blocks.Count} blocks."");
        }
    }
}
