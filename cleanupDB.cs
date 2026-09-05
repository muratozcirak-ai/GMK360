using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using GMK360.Infrastructure.Data;

class Program
{
    static void Main()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=GMK360Db;Trusted_Connection=True;MultipleActiveResultSets=true");
        using (var context = new ApplicationDbContext(optionsBuilder.Options))
        {
            var projects = context.ConstructionProjects
                .Include(p => p.Blocks)
                .Include(p => p.Phases)
                .Include(p => p.Amenities)
                .ToList();
            
            foreach (var p in projects)
            {
                if (p.Blocks != null) context.Buildings.RemoveRange(p.Blocks);
                if (p.Phases != null) context.ProjectPhases.RemoveRange(p.Phases);
                if (p.Amenities != null) context.ProjectAmenities.RemoveRange(p.Amenities);
            }
            context.ConstructionProjects.RemoveRange(projects);
            context.SaveChanges();
            Console.WriteLine("All ConstructionProjects deleted successfully.");
        }
    }
}
