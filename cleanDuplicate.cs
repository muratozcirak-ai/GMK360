using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.Construction;

class Program
{
    static void Main()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=GMK360Db;Trusted_Connection=True;MultipleActiveResultSets=true");

        using (var db = new ApplicationDbContext(optionsBuilder.Options))
        {
            var duplicates = db.ConstructionProjects
                .Where(p => p.Name == "Yaşar Bey Apartman")
                .OrderByDescending(p => p.Id)
                .ToList();

            if (duplicates.Count > 1)
            {
                // Delete the most recent one to keep the older one, or vice versa?
                // Wait, he just created the 2nd one now, so it might not have blocks.
                // Let's delete the one with Id = duplicates.First().Id (the highest ID)
                var toDelete = duplicates.First();
                
                // Soft delete is active now! So we just set IsDeleted=true by calling Remove
                db.ConstructionProjects.Remove(toDelete);
                db.SaveChanges();
                Console.WriteLine($"Deleted duplicate project with ID: {toDelete.Id}");
            }
            else
            {
                Console.WriteLine("No duplicates found.");
            }
        }
    }
}
