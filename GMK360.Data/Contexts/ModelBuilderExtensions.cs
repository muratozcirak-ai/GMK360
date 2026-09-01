using Microsoft.EntityFrameworkCore;
using GMK360.Core.Entities;

namespace GMK360.Data.Contexts
{
    public static class ModelBuilderExtensions
    {
        public static void SeedGMK360Data(this ModelBuilder modelBuilder)
        {
            // 1. Definition Categories (instead of DefinitionTypes)
            modelBuilder.Entity<DefinitionCategory>().HasData(
                new DefinitionCategory { Id = 1, Name = "Konut Tipleri", SystemCode = "PROPERTY_TYPE", IsDeleted = false },
                new DefinitionCategory { Id = 2, Name = "Birim Tipleri", SystemCode = "UNIT_TYPE", IsDeleted = false }
            );

            // 2. Definition Values
            modelBuilder.Entity<DefinitionValue>().HasData(
                new DefinitionValue { Id = 1, CategoryId = 1, Name = "Konut", IsDeleted = false },
                new DefinitionValue { Id = 2, CategoryId = 1, Name = "İş Yeri", IsDeleted = false },
                
                new DefinitionValue { Id = 3, CategoryId = 2, Name = "Daire", IsDeleted = false },
                new DefinitionValue { Id = 4, CategoryId = 2, Name = "Dubleks", IsDeleted = false },
                new DefinitionValue { Id = 5, CategoryId = 2, Name = "Asma Kat", IsDeleted = false },
                new DefinitionValue { Id = 6, CategoryId = 2, Name = "Villa", IsDeleted = false },
                new DefinitionValue { Id = 7, CategoryId = 2, Name = "Dükkan", IsDeleted = false },
                new DefinitionValue { Id = 8, CategoryId = 2, Name = "Depo", IsDeleted = false },
                new DefinitionValue { Id = 9, CategoryId = 2, Name = "Pansiyon Odası", IsDeleted = false }
            );

            // 3. Country
            modelBuilder.Entity<Country>().HasData(
                new Country { Id = 1, Name = "Türkiye", Code = "TR", IsDeleted = false }
            );

            // 4. Cities (İl)
            modelBuilder.Entity<City>().HasData(
                new City { Id = 34, CountryId = 1, Name = "İstanbul", PlateCode = "34", IsDeleted = false },
                new City { Id = 6, CountryId = 1, Name = "Ankara", PlateCode = "06", IsDeleted = false },
                new City { Id = 35, CountryId = 1, Name = "İzmir", PlateCode = "35", IsDeleted = false },
                new City { Id = 7, CountryId = 1, Name = "Antalya", PlateCode = "07", IsDeleted = false }
            );

            // 5. Districts (İlçe)
            modelBuilder.Entity<District>().HasData(
                new District { Id = 1, CityId = 34, Name = "Kadıköy", IsDeleted = false },
                new District { Id = 2, CityId = 34, Name = "Beşiktaş", IsDeleted = false },
                new District { Id = 3, CityId = 6, Name = "Çankaya", IsDeleted = false },
                new District { Id = 4, CityId = 35, Name = "Karşıyaka", IsDeleted = false }
            );
            // 6. Financial Obligation Types (Vergi ve Yükümlülük Kuralları)
            modelBuilder.Entity<FinancialObligationType>().HasData(
                new FinancialObligationType { Id = 1, Name = "Emlak Vergisi", TargetPropertyType = TargetPropertyType.All, ResponsibleRole = ResponsibleRole.Owner, PaymentFrequency = PaymentFrequency.Biannual, FirstInstallmentMonth = 3, SecondInstallmentMonth = 11, IsActive = true, IsDeleted = false },
                new FinancialObligationType { Id = 2, Name = "Kira Gelir Vergisi (GMSİ)", TargetPropertyType = TargetPropertyType.All, ResponsibleRole = ResponsibleRole.Owner, PaymentFrequency = PaymentFrequency.Biannual, FirstInstallmentMonth = 3, SecondInstallmentMonth = 7, IsActive = true, IsDeleted = false },
                new FinancialObligationType { Id = 3, Name = "Çevre Temizlik Vergisi (ÇTV - Daire)", TargetPropertyType = TargetPropertyType.OnlyResidential, ResponsibleRole = ResponsibleRole.Tenant, PaymentFrequency = PaymentFrequency.Monthly, FirstInstallmentMonth = null, SecondInstallmentMonth = null, IsActive = true, IsDeleted = false },
                new FinancialObligationType { Id = 4, Name = "Çevre Temizlik Vergisi (ÇTV - Dükkan)", TargetPropertyType = TargetPropertyType.OnlyCommercial, ResponsibleRole = ResponsibleRole.Tenant, PaymentFrequency = PaymentFrequency.Biannual, FirstInstallmentMonth = 3, SecondInstallmentMonth = 11, IsActive = true, IsDeleted = false },
                new FinancialObligationType { Id = 5, Name = "İlan ve Reklam Vergisi", TargetPropertyType = TargetPropertyType.OnlyCommercial, ResponsibleRole = ResponsibleRole.Tenant, PaymentFrequency = PaymentFrequency.Yearly, FirstInstallmentMonth = 1, SecondInstallmentMonth = null, IsActive = true, IsDeleted = false },
                new FinancialObligationType { Id = 6, Name = "İşyeri Kira Stopajı", TargetPropertyType = TargetPropertyType.OnlyCommercial, ResponsibleRole = ResponsibleRole.Tenant, PaymentFrequency = PaymentFrequency.Monthly, FirstInstallmentMonth = null, SecondInstallmentMonth = null, IsActive = true, IsDeleted = false }
            );
        }
    }
}
