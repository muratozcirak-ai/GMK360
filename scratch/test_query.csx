using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;

var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=GMK360Db;Trusted_Connection=True;MultipleActiveResultSets=true")
    .Options;

using var context = new ApplicationDbContext(options);

var units = context.BuildingUnits
    .Include(u => u.Building)
    .Include(u => u.Property)
    .Where(u => u.Building.ConstructionProjectId == 14)
    .ToList();

Console.WriteLine($"Total Units: {units.Count}");

var flats = units.Where(u => u.Category == "Daire").ToList();
var shops = units.Where(u => u.Category == "Dukkan").ToList();

var givenFlats = flats.Count(u => !string.IsNullOrEmpty(u.OwnerName) || !string.IsNullOrEmpty(u.OwnerUserId));
var givenShops = shops.Count(u => !string.IsNullOrEmpty(u.OwnerName) || !string.IsNullOrEmpty(u.OwnerUserId));

var leftFlats = flats.Count - givenFlats;
var leftShops = shops.Count - givenShops;

Console.WriteLine($"Given Flats: {givenFlats}, Given Shops: {givenShops}");
Console.WriteLine($"Left Flats: {leftFlats}, Left Shops: {leftShops}");

decimal estimatedValue = units.Where(u => string.IsNullOrEmpty(u.OwnerName) && string.IsNullOrEmpty(u.OwnerUserId) && u.Property != null)
                              .Sum(u => u.Property.Price);

Console.WriteLine($"Estimated Value: {estimatedValue}");
