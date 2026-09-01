using System;
using System.Threading.Tasks;
using GMK360.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace GMK360.Data.Seeds
{
    public static class RoleAndUserSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roleNames = { 
                "SuperAdmin", "Admin", "Muhasebe", "TeknikServis", "HalklaIliskiler", 
                "BinaYoneticisi", "Sakin", "Usta", "InsaatFirmasi", "Corporate", "Bireysel" 
            };

            // Roller yoksa oluştur
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
                }
            }

            // Test Kullanıcıları Tanımlaması
            var testUsers = new[]
            {
                new { Email = "kurucu@gmk360.com", FirstName = "Süper", LastName = "Yönetici", Role = "SuperAdmin", UserType = UserType.Corporate, Tc = "11111111111" },
                new { Email = "insaat@gmk360.com", FirstName = "Ahmet", LastName = "Müteahhit", Role = "InsaatFirmasi", UserType = UserType.Corporate, Tc = "22222222222" },
                new { Email = "emlak@gmk360.com", FirstName = "Ayşe", LastName = "Danışman", Role = "Corporate", UserType = UserType.Corporate, Tc = "33333333333" },
                new { Email = "usta@gmk360.com", FirstName = "Mehmet", LastName = "Usta", Role = "Usta", UserType = UserType.Individual, Tc = "44444444444" },
                new { Email = "sakin@gmk360.com", FirstName = "Ali", LastName = "Sakin", Role = "Sakin", UserType = UserType.Individual, Tc = "55555555555" },
                new { Email = "bina@gmk360.com", FirstName = "Fatma", LastName = "Yönetici", Role = "BinaYoneticisi", UserType = UserType.Individual, Tc = "66666666666" }
            };

            foreach (var tUser in testUsers)
            {
                var existingUser = await userManager.FindByEmailAsync(tUser.Email);
                if (existingUser == null)
                {
                    var newUser = new ApplicationUser
                    {
                        UserName = tUser.Email,
                        Email = tUser.Email,
                        FirstName = tUser.FirstName,
                        LastName = tUser.LastName,
                        EmailConfirmed = true,
                        UserType = tUser.UserType,
                        TcIdentityNo = tUser.Tc,
                        BirthYear = 1980,
                        
                        IsEDevletVerified = true, // MERNIS testleri atlatılsın diye
                        RecoveryQuestion = "İlk evcil hayvanınızın adı?",
                        RecoveryAnswer = "Karabaş"
                    };

                    var createResult = await userManager.CreateAsync(newUser, "Test1234*");
                    if (createResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newUser, tUser.Role);
                    }
                    else
                    {
                        var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                        Console.WriteLine($"[SEEDER ERROR] {tUser.Email} oluşturulamadı: {errors}");
                    }
                }
            }
        }
    }
}


