using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using GMK360.Data.Contexts;

var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=GMK360Db;Trusted_Connection=True;MultipleActiveResultSets=true")
    .Options;

using var context = new ApplicationDbContext(options);
var projects = context.ConstructionProjects.ToList();
foreach(var p in projects) {
    Console.WriteLine($"{p.Id} - {p.Name}");
}
