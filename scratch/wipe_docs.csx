using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;

var builder = new ConfigurationBuilder()
    .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "GMK360.Web"))
    .AddJsonFile("appsettings.json");
var configuration = builder.Build();

var services = new ServiceCollection();
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

var serviceProvider = services.BuildServiceProvider();
using var scope = serviceProvider.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

var oldDocs = db.ProjectLegalDocuments.ToList();
db.ProjectLegalDocuments.RemoveRange(oldDocs);
db.SaveChanges();
Console.WriteLine($"Deleted {oldDocs.Count} old project legal documents.");
