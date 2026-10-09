using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using GMK360.Infrastructure.Data;
using GMK360.Core.Entities;

var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=GMK360Db;Trusted_Connection=True;MultipleActiveResultSets=true");

using (var context = new ApplicationDbContext(optionsBuilder.Options))
{
    var project = context.ConstructionProjects.OrderByDescending(p => p.Id).FirstOrDefault();
    if(project != null) {
        var blocks = context.Buildings.Where(b => b.ConstructionProjectId == project.Id && b.ParentBuildingId == null).ToList();
        foreach(var b in blocks) {
            Console.WriteLine($"ID: {b.Id}, Name: {b.Name}, IsTarget: {!b.IsExistingBuilding}, Pattern: {b.BuildingCategory}, Floors: {b.TotalFloors}, Units: {b.TotalUnits}");
        }
    }
}
